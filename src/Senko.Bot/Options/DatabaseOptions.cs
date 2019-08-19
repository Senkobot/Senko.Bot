using System.Data.Common;
using Senko.Framework;

namespace Senko.Bot.Options
{
    [Configuration("Database")]
    public class DatabaseOptions
    {
        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 5432;

        public string Username { get; set; } = "postgres";

        public string Password { get; set; } = "password";

        public string Name { get; set; } = "senko";

        public bool Pooling { get; set; } = true;

        public string GetConnectionString()
        {
            var csb = new DbConnectionStringBuilder
            {
                ["Host"] = Host,
                ["Port"] = Port,
                ["User ID"] = Username,
                ["Password"] = Password,
                ["Database"] = Name,
                ["Pooling"] = Pooling ? "true" : "false",
            };

            return csb.ConnectionString;
        }
    }
}
