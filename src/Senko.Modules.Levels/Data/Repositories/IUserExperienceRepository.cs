using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Senko.Modules.Levels.Data.Entities;

namespace Senko.Modules.Levels.Data.Repositories
{
    public interface IUserExperienceRepository
    {
        Task<UserExperience> GetAsync(ulong guildId, ulong userId);

        Task<IReadOnlyList<UserExperience>> GetAll(ulong guildId);

        Task AddAsync(UserExperience entity);

        Task UpdateAsync(UserExperience entity);
    }
}
