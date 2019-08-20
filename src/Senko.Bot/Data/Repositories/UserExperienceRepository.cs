using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Senko.Modules.Levels.Data.Entities;
using Senko.Modules.Levels.Data.Repositories;

namespace Senko.Bot.Data.Repositories
{
    public class UserExperienceRepository : EfCoreRepository<UserExperience>, IUserExperienceRepository
    {
        public UserExperienceRepository(BotDbContext context) : base(context)
        {
        }

        public Task<UserExperience> GetAsync(ulong guildId, ulong userId)
        {
            return Set.FirstOrDefaultAsync(ue => ue.GuildId == guildId && ue.UserId == userId);
        }

        public async Task<IReadOnlyList<UserExperience>> GetAll(ulong guildId)
        {
            return await Set.ToArrayAsync();
        }
    }
}
