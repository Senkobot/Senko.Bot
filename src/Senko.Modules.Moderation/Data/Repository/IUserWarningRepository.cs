using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Senko.Modules.Moderation.Data.Entities;

namespace Senko.Modules.Moderation.Data.Repository
{
    public interface IUserWarningRepository
    {
        Task<UserWarning> GetAsync(int id);

        Task<IReadOnlyList<UserWarning>> GetWarningsForUserAsync(ulong guildId, ulong userId);

        Task<IReadOnlyList<UserWarning>> GetWarningsByModeratorAsync(ulong guildId, ulong moderatorId);
        
        Task<UserWarning> GetWarningByMessageId(ulong guildId, ulong messageId);

        Task AddAsync(UserWarning entity);

        Task UpdateAsync(UserWarning entity);

        Task RemoveAsync(UserWarning entity);
    }
}
