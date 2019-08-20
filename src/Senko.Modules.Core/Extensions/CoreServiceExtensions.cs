using Microsoft.Extensions.DependencyInjection;
using Senko.Commands;
using Senko.Modules.Core.Modules;

namespace Senko.Modules.Core.Extensions
{
    public static class CoreServiceExtensions
    {
        public static IServiceCollection AddCoreModule(this IServiceCollection services)
        {
            services.AddSingleton<IModule, GuildModule>();
            services.AddSingleton<IModule, PermissionModule>();
            return services;
        }
    }
}
