using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Senko.Bot.Data.Entities;
using Senko.Framework.Repositories;

namespace Senko.Bot.Data.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly TimeSpan _cacheTime = TimeSpan.FromMinutes(10);
        private readonly ICacheClient _cacheClient;
        private readonly IServiceProvider _provider;

        public SettingRepository(ICacheClient cacheClient, IServiceProvider provider)
        {
            _cacheClient = cacheClient;
            _provider = provider;
        }

        public async Task<string> GetAsync(ulong guildId, string key)
        {
            var cacheKey = $"Senko:Settings:{guildId}:{key}";
            var cacheItem = await _cacheClient.GetAsync<string>(cacheKey);

            if (cacheItem.HasValue)
            {
                return cacheItem.Value;
            }

            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BotDbContext>();
            var value = (await context.Settings.FirstOrDefaultAsync(s => s.GuildId == guildId && s.Key == key))?.Value;

            await _cacheClient.SetAsync(cacheKey, value, _cacheTime);

            return value;
        }

        public async Task SetAsync(ulong guildId, string key, string value)
        {
            var cacheKey = $"Senko:Settings:{guildId}:{key}";
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BotDbContext>();
            var entity = (await context.Settings.FirstOrDefaultAsync(s => s.GuildId == guildId && s.Key == key));

            if (entity == null)
            {
                entity = new Setting
                {
                    Key = key,
                    GuildId = guildId,
                    Value = value
                };
                context.Add(entity);
            }
            else
            {
                entity.Value = value;
                context.Update(entity);
            }

            await Task.WhenAll(
                context.SaveChangesAsync(),
                _cacheClient.SetAsync(cacheKey, value, _cacheTime)
            );
        }
    }
}
