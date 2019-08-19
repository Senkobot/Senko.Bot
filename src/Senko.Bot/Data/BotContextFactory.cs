using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Senko.Bot.Options;

namespace Senko.Bot.Data
{
    public class BotContextFactory : IDesignTimeDbContextFactory<BotDbContext>
    {
        public BotDbContext CreateDbContext(string[] args)
        {
            var options = new DatabaseOptions
            {
                Username = "postgres",
                Password = "password",
                Name = "senko"
            };

            return new BotDbContext(new OptionsWrapper<DatabaseOptions>(options));
        }
    }
}
