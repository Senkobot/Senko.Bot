using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Senko.Discord;
using Senko.Discord.Packets;
using Senko.Commands;
using Senko.Events.Attributes;
using Senko.Framework;
using Senko.Framework.Events;
using Senko.Localization;
using Senko.Modules.Levels.Events;
using Senko.Modules.Levels.Managers;
using Senko.Modules.Levels.Models;
using Senko.Modules.Levels.Utility;

namespace Senko.Modules.Levels.Modules
{
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    public class LevelModule : IModule
    {
        private readonly LevelOptions _options;
        private readonly LevelManager _levelManager;
        private readonly IStringLocalizer _localizer;

        public LevelModule(LevelManager levelManager, IStringLocalizer localizer, IOptions<LevelOptions> options)
        {
            _levelManager = levelManager;
            _localizer = localizer;
            _options = options.Value;
        }

        [EventListener]
        public Task HandleMessage(MessageReceivedEvent e)
        {
            if (!_options.EnableLeveling || e.Author.IsBot || !e.GuildId.HasValue)
            {
                return Task.CompletedTask;
            }

            return _levelManager.QueueMessageUserExperienceAsync(e.GuildId.Value, e.Author.Id, e.ChannelId);
        }

        [EventListener]
        public async Task HandleLevelUpAsync(UserLevelUpEvent e, IDiscordClient client)
        {
            if (!_options.EnableLeveling)
            {
                return;
            }

            var user = await client.GetUserAsync(e.UserId);
            var message = _localizer["Level.LevelUp"]
                .WithToken("User", user.Mention)
                .WithToken("NewLevel", e.NewLevel)
                .WithToken("OldLevel", e.OldLevel);

            await client.SendMessageAsync(e.ChannelId, message);
        }

        [Command("level", PermissionGroup.User, GuildOnly = true)]
        public async Task GetLevelAsync(MessageContext context)
        {
            var experience = await _levelManager.GetUserExperienceAsync(context.Request.GuildId.Value, context.User.Id);
            var level = LevelCalculator.GetLevel(experience);
            var remainingExperience = LevelCalculator.GetExperience(level + 1) - experience;

            var sb = new StringBuilder();
            sb.Append($"You have {experience} XP, which is level {level}. ");

            if (level == LevelCalculator.MaxLevel)
            {
                sb.Append("This is the max level. You should consider your life choices.");
            }
            else
            {
                sb.Append($"You only need {remainingExperience} XP for the next level.");
            }

            context.Response.AddMessage(sb.ToString());
        }

        [Command("addxp", PermissionGroup.Developer, GuildOnly = true)]
        public Task AddXp(MessageContext context, [Optional] IDiscordGuildUser user, long amount)
        {
            if (!_options.EnableLeveling)
            {
                context.Response.AddError("Leveling is not enabled.");
                return Task.CompletedTask;
            }

            return _levelManager.QueueUserExperienceAsync(context.Request.GuildId.Value, user?.Id ?? context.User.Id, amount, context.Request.ChannelId);
        }

        [Command("importlevels", PermissionGroup.Administrator, GuildOnly = true)]
        public void ImportLevels(MessageContext context, ulong id)
        {
            if (!_options.EnableLeveling)
            {
                context.Response.AddError("Leveling is not enabled.");
                return;
            }

            context.Response.AddMessage(_localizer["Level.ImportLevels.Importing"])
                .Then(_ => _levelManager.ImportExperienceAsync(context.Request.GuildId.Value, ExperienceImportMode.Replace, id))
                .Then(arg => arg.Builder.SetContent(_localizer["Level.ImportLevels.ImportComplete"]));
        }
    }
}
