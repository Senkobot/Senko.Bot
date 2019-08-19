using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Senko.Bot.Data.Entities;
using Senko.Bot.Options;
using Senko.Commands.EfCore;
using Senko.Events;
using Senko.Events.Attributes;
using Senko.Framework.Events;
using Senko.Modules.Moderation.Data.Entities;

namespace Senko.Bot.Data
{
    public class BotDbContext : DbContext, IEventListener
    {
        private readonly DatabaseOptions _options;

        public BotDbContext(IOptions<DatabaseOptions> options)
        {
            _options = options.Value;
        }

        public DbSet<Setting> Settings { get; set; }

        [EventListener(typeof(InitializeEvent), EventPriority.Highest)]
        public Task InitializeAsync()
        {
            return Database.MigrateAsync();
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            model.AddCommand();

            model.Entity<UserWarning>(builder =>
            {
                builder.HasKey(uw => uw.Id);
            });

            model.Entity<Setting>(builder =>
            {
                builder.HasKey(s => new { s.GuildId, s.Key });
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);

            builder.UseNpgsql(_options.GetConnectionString());
        }
    }
}
