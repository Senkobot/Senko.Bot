using Senko.Events;
using Senko.Framework.Events;

namespace Senko.Modules.Levels.Events
{
    public class UserLevelUpEvent : IGuildEvent
    {
        /// <summary>
        ///     The user ID.
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        ///     The guild ID.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        ///     The channel ID where the user did level up.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        ///     The old level of the user.
        /// </summary>
        public long OldLevel { get; set; }

        /// <summary>
        ///     The new level of the user.
        /// </summary>
        public long NewLevel { get; set; }

        ulong? IGuildEvent.GuildId => GuildId;
    }
}
