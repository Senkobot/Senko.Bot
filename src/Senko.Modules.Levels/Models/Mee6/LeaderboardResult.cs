using Newtonsoft.Json;

namespace Senko.Modules.Levels.Models.Mee6
{
    public class LeaderboardResult
    {
        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("banner_url")]
        public object BannerUrl { get; set; }

        [JsonProperty("guild")]
        public Guild Guild { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("player")]
        public object Player { get; set; }

        [JsonProperty("players")]
        public Player[] Players { get; set; }

        [JsonProperty("role_rewards")]
        public object[] RoleRewards { get; set; }

        [JsonProperty("user_guild_settings")]
        public object UserGuildSettings { get; set; }
    }
}
