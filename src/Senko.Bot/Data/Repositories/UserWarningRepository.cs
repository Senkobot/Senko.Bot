using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Senko.Commands.Repositories;
using Senko.Modules.Moderation.Data.Entities;
using Senko.Modules.Moderation.Data.Repository;

namespace Senko.Bot.Data.Repositories
{
    public class UserWarningRepository : EfCoreRepository<UserWarning>, IUserWarningRepository
    {
        public UserWarningRepository(BotDbContext context)
            : base(context)
        {
        }

        public Task<UserWarning> GetAsync(int id)
        {
            return Set.FirstOrDefaultAsync(uw => uw.Id == id);
        }

        public async Task<IReadOnlyList<UserWarning>> GetWarningsForUserAsync(ulong guildId, ulong userId)
        {
            return await Set.Where(uw => uw.GuildId == guildId && uw.UserId == userId).ToArrayAsync();
        }

        public async Task<IReadOnlyList<UserWarning>> GetWarningsByModeratorAsync(ulong guildId, ulong moderatorId)
        {
            return await Set.Where(uw => uw.GuildId == guildId && uw.ModeratorId == moderatorId).ToArrayAsync();
        }

        public Task<UserWarning> GetWarningByMessageId(ulong guildId, ulong messageId)
        {
            return Set.FirstOrDefaultAsync(uw => uw.GuildId == guildId && uw.ConsoleMessageId == messageId);
        }
    }
}
