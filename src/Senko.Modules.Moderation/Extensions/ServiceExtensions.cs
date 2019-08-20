using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Senko.Commands;
using Senko.Commands.Managers;
using Senko.Localization;
using Senko.Localization.Resources;
using Senko.Modules.Moderation.Services;

namespace Senko.Modules.Moderation
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddModerationModule(this IServiceCollection services)
        {
            services.AddSingleton<IModule, ModerationModule>();
            services.AddSingleton<ConsoleService>();
            services.AddSingleton<IStringRepository>(new ResourceStringRepository(typeof(ModerationModule).Assembly));

            return services;
        }
    }
}
