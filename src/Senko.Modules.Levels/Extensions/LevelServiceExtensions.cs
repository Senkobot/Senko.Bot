using Microsoft.Extensions.DependencyInjection;
using Senko.Commands;
using Senko.Framework;
using Senko.Modules.Levels.Managers;
using Senko.Modules.Levels.Modules;
using Senko.Modules.Levels.Services;

namespace Senko.Modules.Levels
{
    public static class LevelServiceExtensions
    {
        public static IServiceCollection AddLevelModule(this IServiceCollection services)
        {
            services.AddSingleton<LevelManager>();
            services.AddSingleton<IModule, LevelModule>();
            services.AddHostedService<StoreLevelService>();
            return services;
        }
    }
}
