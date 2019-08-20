using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Senko.Commands;
using Senko.Discord;
using Senko.Discord.Packets;
using Senko.Events;
using Senko.Events.Attributes;
using Senko.Framework;
using Senko.Framework.Discord;
using Senko.Framework.Events;
using Senko.Framework.Features;
using Senko.Framework.Repositories;
using Senko.Localization;
using Senko.Modules.Moderation.Data.Entities;
using Senko.Modules.Moderation.Data.Repository;
using Senko.Modules.Moderation.Events;
using Senko.Modules.Moderation.Services;

namespace Senko.Modules.Moderation
{
    [DefaultModule]
    public class ModerationModule : IModule
    {
        private readonly IStringLocalizer _localizer;
        private readonly IUserWarningRepository _warningRepository;
        private readonly IEventManager _eventManager;
        private readonly ConsoleService _consoleService;

        public ModerationModule(
            IStringLocalizer localizer,
            IUserWarningRepository warningRepository,
            IEventManager eventManager, 
            ConsoleService consoleService)
        {
            _localizer = localizer;
            _warningRepository = warningRepository;
            _eventManager = eventManager;
            _consoleService = consoleService;
        }

        [Command("warn", PermissionGroup.Moderator, GuildOnly = true)]
        public async Task WarnAsync(
            IDiscordGuild guild,
            MessageContext context,
            IDiscordGuildUser user,
            [Optional, Remaining] string reason)
        {
            var @event = await _eventManager.CallAsync(new UserWarnEvent(user, context.User, reason));

            if (@event.IsCancelled)
            {
                return;
            }

            var warning = new UserWarning
            {
                GuildId = guild.Id,
                UserId = user.Id,
                ModeratorId = context.User.Id,
                Reason = reason,
                Created = DateTime.UtcNow
            };

            await _warningRepository.AddAsync(warning);
            await _consoleService.UpdateWarningMessageAsync(context, warning, user, context.User);

            var message = _localizer["Moderation.Warn.Message"].WithToken("user", user);
            var displayReason = _consoleService.GetDisplayReason(warning);

            var footer = _localizer["Moderation.Warn.Footer"]
                .WithToken("moderator", context.User);

            context.Response.AddSuccess(message)
                .SetEmbedFooter(footer, context.User.GetAvatarUrl(size: ImageSize.x32))
                .AddEmbedField(_localizer["Moderation.Warn.Reason"], displayReason);
        }

        [Command("reason", PermissionGroup.Moderator, GuildOnly = true)]
        public async Task ReasonAsync(IDiscordGuild guild, MessageContext context, int id, [Remaining] string reason)
        {
            var warning = await _warningRepository.GetAsync(id);

            if (warning == null)
            {
                context.Response.AddError(_localizer["Moderation.Warn.NotFound"].WithToken("id", id));
                return;
            }

            var user = await guild.GetMemberAsync(warning.UserId);

            warning.Reason = reason;

            await _warningRepository.UpdateAsync(warning);
            await _consoleService.UpdateWarningMessageAsync(context, warning, user, context.User);

            context.Response.React(Emoji.WhiteCheckMark);
        }

        [Command("warnings", PermissionGroup.Moderator, GuildOnly = true)]
        public async Task WarningsAsync(IDiscordGuild guild, MessageContext context, IDiscordGuildUser user)
        {
            var warnings = await _warningRepository.GetWarningsForUserAsync(guild.Id, user.Id);

            string GetLine(UserWarning w)
            {
                var title = w.ConsoleChannelId.HasValue && w.ConsoleMessageId.HasValue
                    ? $"[#{w.Id}](https://discordapp.com/channels/{guild.Id}/{w.ConsoleChannelId}/{w.ConsoleMessageId})"
                    : $"#{w.Id}";

                return $"[{title} | {w.Created:d}] {w.Reason.MaxLength(50, "...")}";
            }

            var lastYear = DateTime.UtcNow.Subtract(TimeSpan.FromDays(365));
            var lastMonth = DateTime.UtcNow.Subtract(TimeSpan.FromDays(31));
            var lastWeek = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7));

            var stats = _localizer["Moderation.Warnings.Stats"]
                .WithToken("total", warnings.Count)
                .WithToken("year", warnings.Count(w => w.Created >= lastYear))
                .WithToken("month", warnings.Count(w => w.Created >= lastMonth))
                .WithToken("week", warnings.Count(w => w.Created >= lastWeek));

            var warningText = warnings.Count > 0
                ? string.Join("\n", warnings.Select(GetLine))
                : _localizer["Moderation.Warnings.Nothing"].ToString();

            context.Response.AddEmbed()
                .SetEmbedAuthor(_localizer["Moderation.Warnings.Title"].WithToken("user", user), user.GetAvatarUrl(size: ImageSize.x32))
                .AddEmbedField(_localizer["Moderation.Warnings.Stats.Header"], stats)
                .AddEmbedField(_localizer["Moderation.Warnings.Header"], warningText);
        }

        [Command("setconsolechannel", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task SetConsoleChannelAsync(IDiscordGuild guild, MessageContext context)
        {
            await _consoleService.SetChannelId(guild.Id, context.Request.ChannelId);

            context.Response.AddSuccess(_localizer["Moderation.Console.ChannelChanged"]);
        }

        [EventListener]
        public async Task OnMessageDeleteEvent(MessageDeleteEvent e, IDiscordClient client)
        {
            if (!e.GuildId.HasValue)
            {
                return;
            }

            var channelId = await _consoleService.GetChannelId(e.GuildId.Value);

            if (e.ChannelId != channelId)
            {
                return;
            }
            
            var warning = await _warningRepository.GetWarningByMessageId(e.GuildId.Value, e.MessageId);

            if (warning == null)
            {
                return;
            }

            warning.ConsoleMessageId = null;
            warning.ConsoleChannelId = null;
            await _warningRepository.UpdateAsync(warning);

            var user = await client.GetUserAsync(warning.UserId);
            var moderator = await client.GetUserAsync(warning.ModeratorId);

            await _consoleService.UpdateWarningMessageAsync(e.GuildId.Value, e.ChannelId, warning, user, moderator);
        }
    }
}
