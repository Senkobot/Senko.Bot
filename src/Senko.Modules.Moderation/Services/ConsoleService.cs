using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Senko.Discord;
using Senko.Discord.Packets;
using Senko.Framework;
using Senko.Framework.Discord;
using Senko.Framework.Features;
using Senko.Framework.Repositories;
using Senko.Localization;
using Senko.Modules.Moderation.Data.Entities;
using Senko.Modules.Moderation.Data.Repository;

namespace Senko.Modules.Moderation.Services
{
    public class ConsoleService
    {
        private readonly IStringLocalizer _localizer;
        private readonly ISettingRepository _settingRepository;
        private readonly IMessageContextDispatcher _messageContextDispatcher;
        private readonly IServiceProvider _provider;

        public ConsoleService(
            IStringLocalizer localizer,
            ISettingRepository settingRepository,
            IMessageContextDispatcher messageContextDispatcher,
            IServiceProvider provider)
        {
            _localizer = localizer;
            _settingRepository = settingRepository;
            _messageContextDispatcher = messageContextDispatcher;
            _provider = provider;
        }

        /// <summary>
        /// Get the console channel ID.
        /// </summary>
        /// <param name="guildId">The guild ID.</param>
        /// <returns>The channel ID of the console.</returns>
        public Task<ulong> GetChannelId(ulong guildId)
        {
            return _settingRepository.GetUInt64Async(guildId, "Moderation.ConsoleChannel");
        }

        /// <summary>
        /// Set the console channel ID for the given guild ID.
        /// </summary>
        /// <param name="guildId">The guild ID.</param>
        /// <param name="channelId">The new channel ID.</param>
        public Task SetChannelId(ulong guildId, ulong channelId)
        {
            return _settingRepository.SetAsync(guildId, "Moderation.ConsoleChannel", channelId);
        }

        /// <summary>
        /// Add or update the warning message in Discord.
        /// </summary>
        /// <param name="guildId">The current guild ID.</param>
        /// <param name="channelId">The current channel ID.</param>
        /// <param name="warning">The warning to update.</param>
        /// <param name="user">The warned user.</param>
        /// <param name="moderator">The moderator.</param>
        /// <returns>The message builder.</returns>
        public async Task UpdateWarningMessageAsync(
            ulong guildId,
            ulong channelId,
            UserWarning warning,
            IDiscordUser user,
            IDiscordUser moderator)
        {
            using var scope = _provider.CreateScope();
            var features = new FeatureCollection();

            features.Set<IServiceProvidersFeature>(new ServiceProvidersFeature(scope.ServiceProvider));
            features.Set<IMessageRequestFeature>(new MessageRequestFeature
            {
                GuildId = guildId,
                ChannelId = channelId
            });

            await _messageContextDispatcher.DispatchAsync(
                context => UpdateWarningMessageAsync(context, warning, user, moderator),
                features
            );
        }

        /// <summary>
        /// Add or update the warning message in Discord.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="warning">The warning to update.</param>
        /// <param name="user">The warned user.</param>
        /// <param name="moderator">The moderator.</param>
        /// <returns>The message builder.</returns>
        public async Task UpdateWarningMessageAsync(
            MessageContext context,
            UserWarning warning,
            IDiscordUser user,
            IDiscordUser moderator)
        {
            var title = _localizer["Moderation.Warn.Title"]
                .WithToken("user", user)
                .WithToken("id", warning.Id);

            var footer = _localizer["Moderation.Warn.Footer"]
                .WithToken("moderator", moderator);
            
            var displayReason = GetDisplayReason(warning);

            bool updateEntity;
            MessageBuilder builder;

            if (warning.ConsoleChannelId.HasValue && warning.ConsoleMessageId.HasValue)
            {
                builder = context.Response.Edit(warning.ConsoleChannelId.Value, warning.ConsoleMessageId.Value);
                updateEntity = false;
            }
            else
            {
                var guildId = context.Request.GuildId ?? throw new InvalidOperationException("This method can only be executed in guilds");
                var channelId = await GetChannelId(guildId);

                if (channelId == 0)
                {
                    context.Response.AddError(_localizer["Moderation.Warnings.NoConsole"]);
                    return;
                }

                builder = context.Response.AddEmbed(channelId: channelId);
                updateEntity = true;
            }

            builder
                .SetEmbedColor(255, 235, 59)
                .SetEmbedAuthor(title, user.GetAvatarUrl(size: ImageSize.x32))
                .SetEmbedFooter(footer, moderator.GetAvatarUrl(size: ImageSize.x32))
                .AddEmbedField(_localizer["Moderation.Warn.User"], $"{user.Username}#{user.Discriminator} ({user.Mention})", true)
                .AddEmbedField(_localizer["Moderation.Warn.UserId"], user.Id, true)
                .AddEmbedField(_localizer["Moderation.Warn.Date"], warning.Created, true)
                .AddEmbedField(_localizer["Moderation.Warn.Reason"], displayReason)
                .Catch(args =>
                {
                    context.Response.AddError(_localizer["Moderation.Warn.ConsoleNotAvailable"]);
                });

            if (updateEntity)
            {
                builder.Then(async args =>
                {
                    var warningRepo = context.RequestServices.GetRequiredService<IUserWarningRepository>();

                    warning.ConsoleMessageId = args.Message.Id;
                    warning.ConsoleChannelId = args.Message.ChannelId;

                    await warningRepo.UpdateAsync(warning);
                });
            }
        }

        public string GetDisplayReason(UserWarning warning)
        {
            string displayReason;

            if (!string.IsNullOrEmpty(warning.Reason))
            {
                displayReason = warning.Reason;
            }
            else
            {
                displayReason = _localizer["Moderation.Warn.NoReason"]
                    .WithToken("id", warning.Id)
                    .WithToken("prefix", ">") // TODO: Get the current prefix.
                    .ToString();
            }

            return displayReason;
        }
    }
}
