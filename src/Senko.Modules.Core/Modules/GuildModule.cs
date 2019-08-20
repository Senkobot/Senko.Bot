using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Senko.Commands;
using Senko.Commands.Managers;
using Senko.Discord;
using Senko.Framework;
using Senko.Localization;

namespace Senko.Modules.Core.Modules
{
    [CoreModule]
    public class GuildModule : IModule
    {
        private readonly IModuleManager _moduleManager;
        private readonly IStringLocalizer _localizer;

        public GuildModule(IModuleManager moduleManager, IStringLocalizer localizer)
        {
            _moduleManager = moduleManager;
            _localizer = localizer;
        }

        [Command("modules", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task ModulesAsync(MessageContext context, IDiscordGuild guild)
        {
            var enabledModules = await _moduleManager.GetEnabledModulesAsync(guild.Id);
            var disabledModules = string.Join(", ", _moduleManager.ModuleNames.Where(m => !enabledModules.Contains(m)));

            context.Response.AddEmbed(_localizer["Guild.Modules.Title"])
                .AddEmbedField(_localizer["Guild.Modules.Enabled.Title"], string.Join(", ", enabledModules))
                .AddEmbedField(_localizer["Guild.Modules.Disabled.Title"], string.Join(", ", disabledModules));
        }

        [Command("enable", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task EnableAsync(MessageContext context, IDiscordGuild guild, string moduleName)
        {
            try
            {
                await _moduleManager.SetModuleEnabledAsync(guild.Id, moduleName, true);

                context.Response.AddMessage(
                    _localizer["Guild.Modules.Enable.Success"]
                        .WithToken("Name", moduleName)
                );
            }
            catch (KeyNotFoundException)
            {
                context.Response.AddError(
                    _localizer["Guild.Modules.ModuleNotFound"]
                        .WithToken("Name", moduleName)
                );
            }
            catch (InvalidOperationException)
            {
                context.Response.AddError(
                    _localizer["Guild.Modules.CannotDisable"]
                        .WithToken("Name", moduleName)
                );
            }
        }

        [Command("disable", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task DisableAsync(MessageContext context, IDiscordGuild guild, string moduleName)
        {
            try
            {
                await _moduleManager.SetModuleEnabledAsync(guild.Id, moduleName, false);

                context.Response.AddMessage(
                    _localizer["Guild.Modules.Disable.Success"]
                        .WithToken("Name", moduleName)
                );
            }
            catch (KeyNotFoundException)
            {
                context.Response.AddError(
                    _localizer["Guild.Modules.ModuleNotFound"]
                        .WithToken("Name", moduleName)
                );
            }
            catch (InvalidOperationException)
            {
                context.Response.AddError(
                    _localizer["Guild.Modules.CannotDisable"]
                        .WithToken("Name", moduleName)
                );
            }
        }
    }
}
