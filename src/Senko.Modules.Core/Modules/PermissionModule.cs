using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Senko.Commands;
using Senko.Commands.Managers;
using Senko.Discord;
using Senko.Framework;
using Senko.Localization;

namespace Senko.Modules.Core.Modules
{
    [CoreModule]
    public class PermissionModule : IModule
    {
        private readonly IPermissionManager _permissionManager;
        private readonly IStringLocalizer _localizer;
        private readonly IModuleManager _moduleManager;

        public PermissionModule(IPermissionManager permissionManager, IStringLocalizer localizer, IModuleManager moduleManager)
        {
            _permissionManager = permissionManager;
            _localizer = localizer;
            _moduleManager = moduleManager;
        }

        private bool ValidatePermission(string permission, MessageContext context)
        {
            if (_permissionManager.Permissions.Contains(permission))
            {
                return true;
            }

            context.Response.AddError($"The permission {permission} doesn't exists.");
            return false;
        }

        [Command("permissions", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task PermissionsAsync(MessageContext context, IDiscordGuild guild, [Optional] IDiscordGuildUser user)
        {
            if (user == null)
            {
                user = (IDiscordGuildUser) context.User;
            }

            var guildId = guild.Id;
            var permissions = _permissionManager.Permissions;
            var allowedPermissions = await _permissionManager.GetAllowedUserPermissionAsync(user.Id, guildId);
            var allowedChannelPermissions = await _permissionManager.GetAllowedChannelPermissionAsync(context.Request.ChannelId, guildId);
            var userGroups = await _permissionManager.GetPermissionGroups(user.Id, guildId);
            var message = context.Response.AddMessage($"The user {user.GetDisplayName()} has access to the groups {string.Join(", ", userGroups)}\nand has the following permissions:");
            var enabledModules = await _moduleManager.GetEnabledModulesAsync(guildId);
            var modules = permissions
                .GroupBy(p => p.Substring(0, p.IndexOf('.')))
                .Where(g => enabledModules.Contains(g.Key, StringComparer.OrdinalIgnoreCase))
                .OrderBy(g => g.Count())
                .ToArray();

            string GetAllowedEmoji(string p)
            {
                if (!allowedPermissions.Contains(p))
                {
                    return "<:no_permission:593133110049243161>";
                }

                if (!allowedChannelPermissions.Contains(p))
                {
                    return "<:no_channel_permission:593133939405881344>";
                }

                return "<:has_permission:593133110020014098>";
            }

            foreach (var group in modules)
            {
                var items = group.Select(p => GetAllowedEmoji(p) + " " + p.Substring(p.IndexOf('.') + 1));

                message.AddEmbedField(group.Key, string.Join('\n', items), true);
            }

            var remaining = modules.Length % 3;

            for (var i = remaining; i >= 0; i--)
            {
                message.AddEmbedField("‌‌ ", "‌‌ ", true);
            }
        }

        [Command("grant", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task GrantAsync(MessageContext context, IDiscordGuildUser user, string permission)
        {
            if (!ValidatePermission(permission, context))
            {
                return;
            }

            if (!await _permissionManager.GrantUserPermissionAsync(user.GuildId, user.Id, permission))
            {
                context.Response.AddError("Failed to grant the permission. Maybe the user already had this permission?");
            }
            else
            {
                context.Response.AddMessage("Success! The user should have the permission now.");
            }
        }

        [Command("grant", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task GrantAsync(MessageContext context, IDiscordGuild guild, IDiscordRole role, string permission)
        {
            if (!ValidatePermission(permission, context))
            {
                return;
            }

            if (!await _permissionManager.GrantRolePermissionAsync(guild.Id, role.Id, permission))
            {
                context.Response.AddError("Failed to grant the permission. Maybe the role already had this permission?");
            }
            else
            {
                context.Response.AddMessage("Success! The role should have the permission now.");
            }
        }

        [Command("grant", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task GrantAsync(MessageContext context, IDiscordGuild guild, IDiscordChannel channel, string permission)
        {
            if (!ValidatePermission(permission, context))
            {
                return;
            }

            if (!await _permissionManager.GrantChannelPermissionAsync(guild.Id, channel.Id, permission))
            {
                context.Response.AddError("Failed to grant the permission. Maybe the channel already had this permission?");
            }
            else
            {
                context.Response.AddMessage("Success! The channel should have the permission now.");
            }
        }

        [Command("revoke", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task RevokeAsync(MessageContext context, IDiscordGuild guild, IDiscordGuildUser user, string permission)
        {
            if (!ValidatePermission(permission, context))
            {
                return;
            }

            if (!await _permissionManager.RevokeUserPermissionAsync(user.GuildId, user.Id, permission))
            {
                context.Response.AddError("Failed to revoke the permission. Maybe the user didn't have the permission in the first place?");
            }
            else
            {
                context.Response.AddMessage("Success! The user should not have the permission anymore.");
            }
        }

        [Command("revoke", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task RevokeAsync(MessageContext context, IDiscordGuild guild, IDiscordRole role, string permission)
        {
            if (!ValidatePermission(permission, context))
            {
                return;
            }

            if (!await _permissionManager.RevokeRolePermissionAsync(guild.Id, role.Id, permission))
            {
                context.Response.AddError("Failed to revoke the permission. Maybe the role didn't have the permission in the first place?");
            }
            else
            {
                context.Response.AddMessage("Success! The role should not have the permission anymore.");
            }
        }

        [Command("revoke", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task RevokeAsync(MessageContext context, IDiscordGuild guild, IDiscordChannel channel, string permission)
        {
            if (!ValidatePermission(permission, context))
            {
                return;
            }

            if (!await _permissionManager.RevokeChannelPermissionAsync(guild.Id, channel.Id, permission))
            {
                context.Response.AddError("Failed to revoke the permission. Maybe the channel didn't have the permission in the first place?");
            }
            else
            {
                context.Response.AddMessage("Success! The channel should not have the permission anymore.");
            }
        }

        [Command("reset", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task ResetAsync(MessageContext context, IDiscordGuild guild, IDiscordGuildUser user, string permission)
        {
            if (!ValidatePermission(permission, context))
            {
                return;
            }

            if (!await _permissionManager.ResetUserPermissionAsync(guild.Id, user.Id, permission))
            {
                context.Response.AddError("Failed to reset the permission. Maybe the user didn't have the permission in the first place?");
            }
            else
            {
                context.Response.AddMessage("Success! The user should have the default permission.");
            }
        }

        [Command("reset", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task ResetAsync(MessageContext context, IDiscordGuild guild, IDiscordRole role, string permission)
        {
            if (!ValidatePermission(permission, context))
            {
                return;
            }

            if (!await _permissionManager.ResetRolePermissionAsync(guild.Id, role.Id, permission))
            {
                context.Response.AddError("Failed to reset the permission. Maybe the role didn't have the permission in the first place?");
            }
            else
            {
                context.Response.AddMessage("Success! The role should have the default permission.");
            }
        }

        [Command("reset", PermissionGroup.Administrator, GuildOnly = true)]
        public async Task ResetAsync(MessageContext context, IDiscordGuild guild, IDiscordChannel channel, string permission)
        {
            if (!ValidatePermission(permission, context))
            {
                return;
            }

            if (!await _permissionManager.ResetChannelPermissionAsync(guild.Id, channel.Id, permission))
            {
                context.Response.AddError("Failed to reset the permission. Maybe the channel didn't have the permission in the first place?");
            }
            else
            {
                context.Response.AddMessage("Success! The channel should have the default permission.");
            }
        }
    }
}
