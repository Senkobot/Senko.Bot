using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.Extensions.DependencyInjection;
using Senko.Discord;
using Newtonsoft.Json;
using Senko.Common;
using Senko.Common.Structs;
using Senko.Events;
using Senko.Modules.Levels.Data.Entities;
using Senko.Modules.Levels.Data.Repositories;
using Senko.Modules.Levels.Events;
using Senko.Modules.Levels.Models;
using Senko.Modules.Levels.Models.Mee6;
using Senko.Modules.Levels.Utility;

namespace Senko.Modules.Levels.Managers
{
    public class LevelManager
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ICacheClient _cache;
        private readonly IServiceProvider _provider;
        private readonly ConcurrentDictionary<GuildUserKey, QueuedUserExperience> _queuedUserExperiences;
        private readonly SemaphoreSlim _storeLock = new SemaphoreSlim(1, 1);
        private readonly IDiscordClient _client;

        public LevelManager(ICacheClient cache, IServiceProvider provider, IDiscordClient client)
        {
            _cache = cache;
            _provider = provider;
            _client = client;
            _queuedUserExperiences = new ConcurrentDictionary<GuildUserKey, QueuedUserExperience>();
        }

        public static TimeSpan CacheTime { get; set; } = TimeSpan.FromMinutes(5);
        
        /// <summary>
        ///     Get the experience of the user.
        /// </summary>
        /// <param name="guildId">The guild ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The amount of experience the user has.</returns>
        public async Task<long> GetUserExperienceAsync(ulong guildId, ulong userId)
        {
            var cacheKey = CacheKey.GetUserExperienceCacheKey(guildId, userId);
            var cache = await _cache.GetAsync<long>(cacheKey);

            if (cache.HasValue)
            {
                return cache.Value;
            }

            using var scope = _provider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserExperienceRepository>();

            var entity = await repository.GetAsync(guildId, userId);
            var experience = entity?.Experience ?? 0;

            await _cache.SetAsync(cacheKey, experience, CacheTime);

            return experience;
        }

        private async Task<QueuedUserExperience> GetQueuedUserExperienceAsync(ulong guildId, ulong userId)
        {
            var key = new GuildUserKey(guildId, userId);
            if (_queuedUserExperiences.TryGetValue(key, out var queuedUserExperience))
            {
                return queuedUserExperience;
            }

            using var scope = _provider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserExperienceRepository>();
            var xp = (await repository.GetAsync(guildId, userId))?.Experience ?? 0;

            queuedUserExperience = new QueuedUserExperience(key, xp, _provider.GetRequiredService<IEventManager>());

            return _queuedUserExperiences.AddOrUpdate(key, queuedUserExperience, (_, current) => current);
        }

        /// <summary>
        ///     Queue the <see cref="experience"/> for the given <see cref="userId"/>.
        /// </summary>
        /// <param name="guildId">The guild ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="experience">The experience to add.</param>
        /// <param name="channelId">The channel where the user currently is.</param>
        /// <returns>The total experience that the user currently has.</returns>
        public async Task<long> QueueUserExperienceAsync(ulong guildId, ulong userId, long experience, ulong channelId)
        {
            return (await GetQueuedUserExperienceAsync(guildId, userId)).AddExperience(experience, channelId, _client);
        }
        
        /// <summary>
        ///     Queue the message experience for the given <see cref="userId"/>.
        /// </summary>
        /// <param name="guildId">The guild ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="channelId">The channel where the user currently is.</param>
        /// <returns>The total experience that the user currently has.</returns>
        public async Task<long> QueueMessageUserExperienceAsync(ulong guildId, ulong userId, ulong channelId)
        {
            return (await GetQueuedUserExperienceAsync(guildId, userId)).AddMessageExperience(channelId, _client);
        }

        /// <summary>
        ///     Store the queued experience of the users.
        /// </summary>
        public async Task StoreQueueAsync()
        {
            if (_queuedUserExperiences.IsEmpty)
            {
                return;
            }

            await _storeLock.WaitAsync();

            try
            {
                using var scope = _provider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IUserExperienceRepository>();
                var cacheItems = new Dictionary<string, long>();
                var now = DateTimeOffset.Now;

                foreach (var kv in _queuedUserExperiences)
                {
                    var item = kv.Value;

                    // If the user did not gain experience the last 5 minutes, remove the user from memory.
                    if (now.Subtract(item.LastExperienceGained).TotalMinutes >= 5)
                    {
                        _queuedUserExperiences.TryRemove(kv.Key, out _);
                    }

                    // Update the experience in the cache and database.
                    var amount = item.GetAndClear();

                    if (amount == 0)
                    {
                        continue;
                    }

                    var userExperience = await repository.GetAsync(item.GuildId, item.UserId);
                    
                    if (userExperience == null)
                    {
                        userExperience = new UserExperience
                        {
                            GuildId = item.GuildId,
                            UserId = item.UserId,
                            Experience = amount
                        };

                        await repository.AddAsync(userExperience);
                    }
                    else
                    {
                        userExperience.Experience += amount;
                        await repository.UpdateAsync(userExperience);
                    }

                    item.CurrentExperience = userExperience.Experience;

                    cacheItems.Add(CacheKey.GetUserExperienceCacheKey(item.GuildId, item.UserId), userExperience.Experience);
                }

                await _cache.SetAllAsync(cacheItems, CacheTime);
            }
            finally
            {
                _storeLock.Release();
            }
        }

        public async Task ImportExperienceAsync(ulong guildId, ExperienceImportMode mode = ExperienceImportMode.Add, ulong? fromGuildId = null)
        {
            using var scope = _provider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserExperienceRepository>();
            var sourceGuildId = fromGuildId ?? guildId;
            var cacheItems = new Dictionary<string, long>();
            var users = (await repository.GetAll(guildId)).ToDictionary(ue => ue.UserId, ue => ue);

            for (var page = 0; ; page++)
            {
                var response = await _httpClient.GetAsync($"https://mee6.xyz/api/plugins/levels/leaderboard/{sourceGuildId}?page={page}");
                var responseText = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<LeaderboardResult>(responseText);

                if (data.Players.Length == 0)
                {
                    break;
                }

                foreach (var player in data.Players)
                {
                    var userId = ulong.Parse(player.Id);
                    var xp = player.Xp;

                    if (users.TryGetValue(userId, out var userExperience))
                    {
                        switch (mode)
                        {
                            case ExperienceImportMode.Replace:
                                userExperience.Experience = xp;
                                break;
                            case ExperienceImportMode.Add:
                                userExperience.Experience += xp;
                                break;
                            case ExperienceImportMode.Subtract:
                                if (userExperience.Experience < xp)
                                {
                                    userExperience.Experience = 0;
                                } 
                                else 
                                {
                                    userExperience.Experience -= xp;
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                        }

                        await repository.UpdateAsync(userExperience);
                    }
                    else
                    {
                        userExperience = new UserExperience
                        {
                            UserId = userId,
                            GuildId = guildId
                        };

                        switch (mode)
                        {
                            case ExperienceImportMode.Replace:
                            case ExperienceImportMode.Add:
                                userExperience.Experience = xp;
                                break;
                            case ExperienceImportMode.Subtract:
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                        }

                        await repository.AddAsync(userExperience);
                    }

                    cacheItems.Add(CacheKey.GetUserExperienceCacheKey(guildId, userId), userExperience.Experience);
                }
            }

            await _cache.SetAllAsync(cacheItems, CacheTime);
        }

        #region Utility classes
        private class QueuedUserExperience
        {
            private readonly IEventManager _eventManager;
            private readonly object _lock = new object();
            private DateTimeOffset? _lastMessageExperience;
            private long _queuedExperience;
            private long _currentLevel;
            private long _nextLevelExperience;

            public QueuedUserExperience(GuildUserKey key, long currentExperience, IEventManager eventManager)
            {
                _eventManager = eventManager;
                GuildId = key.GuildId;
                UserId = key.UserId;
                CurrentExperience = currentExperience;
                _currentLevel = LevelCalculator.GetLevel(currentExperience);
                _nextLevelExperience = LevelCalculator.GetExperience(_currentLevel + 1);
            }

            public ulong GuildId { get; }

            public ulong UserId { get; }

            public DateTimeOffset LastExperienceGained { get; set; }

            public long CurrentExperience { get; set; }

            public bool IsMessageExperienceAvailable => _lastMessageExperience == null || DateTimeOffset.Now.Subtract(_lastMessageExperience.Value).TotalMinutes >= 1d;

            private void CheckNewLevel(ulong channelId, IDiscordClient client)
            {
                if (_currentLevel == LevelCalculator.MaxLevel || CurrentExperience < _nextLevelExperience)
                {
                    return;
                }

                var oldLevel = _currentLevel;

                // If the user was given a lot of XP it is possible the user gained multiple levels.
                do
                {
                    _currentLevel++;
                    _nextLevelExperience = LevelCalculator.GetExperience(_currentLevel + 1);
                } while (_currentLevel < LevelCalculator.MaxLevel && CurrentExperience >= _nextLevelExperience);
                
                // Call the event manager.
                Task.Run(() => _eventManager.CallAsync(new UserLevelUpEvent
                {
                    UserId = UserId,
                    GuildId = GuildId,
                    ChannelId = channelId,
                    OldLevel = oldLevel,
                    NewLevel = _currentLevel
                }));
            }

            public long AddExperience(long amount, ulong channelId, IDiscordClient client)
            {
                lock (_lock)
                {
                    _queuedExperience += amount;
                    CurrentExperience += amount;
                    CheckNewLevel(channelId, client);
                    LastExperienceGained = DateTimeOffset.Now;
                    return _queuedExperience;
                }
            }

            public long AddMessageExperience(ulong channelId, IDiscordClient client)
            {
                if (!IsMessageExperienceAvailable)
                {
                    return _queuedExperience;
                }

                lock (_lock)
                {
                    if (!IsMessageExperienceAvailable)
                    {
                        return _queuedExperience;
                    }

                    var amount = StaticRandom.Instance.Next(15, 25);

                    _queuedExperience += amount;
                    CurrentExperience += amount;
                    _lastMessageExperience = DateTimeOffset.Now;
                    LastExperienceGained = DateTimeOffset.Now;
                    CheckNewLevel(channelId, client);

                    return _queuedExperience;
                }
            }

            public long GetAndClear()
            {
                lock (_lock)
                {
                    var experience = _queuedExperience;
                    _queuedExperience = 0;
                    return experience;
                }
            }
        }
        #endregion
    }
}
