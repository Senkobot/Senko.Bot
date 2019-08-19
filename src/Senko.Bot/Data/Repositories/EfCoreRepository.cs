using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Senko.Bot.Data.Repositories
{
    public class EfCoreRepository<TEntity>
        where TEntity : class
    {
        public EfCoreRepository(BotDbContext context)
        {
            Context = context;
            Set = context.Set<TEntity>();
        }

        protected DbSet<TEntity> Set { get; }

        protected BotDbContext Context { get; }

        public Task AddAsync(TEntity entity)
        {
            Context.Add(entity);
            
            return Context.SaveChangesAsync();
        }

        public Task UpdateAsync(TEntity entity)
        {
            Context.Update(entity);

            return Context.SaveChangesAsync();
        }

        public Task RemoveAsync(TEntity entity)
        {
            Context.Remove(entity);
            
            return Context.SaveChangesAsync();
        }
    }
}
