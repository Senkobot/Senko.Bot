using System;
using System.Collections.Generic;
using System.Text;

namespace Senko.Modules.Moderation.Data.Entities
{
    public class UserWarning
    {
        public int Id { get; set; }

        public ulong GuildId { get; set; }

        public ulong UserId { get; set; }

        public ulong ModeratorId { get; set; }

        public ulong? ConsoleMessageId { get; set; }

        public ulong? ConsoleChannelId { get; set; }

        public string Reason { get; set; }

        public DateTime Created { get; set; }
    }
}
