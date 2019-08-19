using System;
using System.Collections.Generic;
using System.Text;
using Senko.Discord;
using Senko.Events;

namespace Senko.Modules.Moderation.Events
{
    public class UserWarnEvent : IEventCancelable
    {
        public UserWarnEvent(IDiscordUser user, IDiscordUser moderator, string reason)
        {
            User = user;
            Moderator = moderator;
            Reason = reason;
        }

        public IDiscordUser User { get; }

        public IDiscordUser Moderator { get; }

        public string Reason { get; }

        public bool IsCancelled { get; set; }
    }
}
