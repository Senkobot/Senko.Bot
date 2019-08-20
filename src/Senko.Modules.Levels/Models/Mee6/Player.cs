using Newtonsoft.Json;

namespace Senko.Modules.Levels.Models.Mee6
{
    public class Player
    {
        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("detailed_xp")]
        public long[] DetailedXp { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty("guild_id")]
        public string GuildId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("xp")]
        public long Xp { get; set; }
    }
}