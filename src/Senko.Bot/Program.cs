using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Senko.Arguments;
using Senko.Bot.Data;
using Senko.Bot.Data.Repositories;
using Senko.Commands;
using Senko.Commands.EfCore;
using Senko.Framework;
using Senko.Framework.Hosting;
using Senko.Framework.Repositories;
using Senko.Localization;
using Senko.Modules.Core.Extensions;
using Senko.Modules.Levels;
using Senko.Modules.Levels.Data.Repositories;
using Senko.Modules.Moderation;
using Senko.Modules.Moderation.Data.Repository;

namespace Senko.Bot
{
    internal class Program
    {
        public static Task Main(string[] args)
        {
            return new BotHostBuilder()
                .ConfigureService(services =>
                {
                    // Core
                    services.AddArgumentWithParsers();
                    services.AddCommand();
                    services.AddLocalizations();

                    // Modules
                    services.AddCoreModule();
                    services.AddModerationModule();
                    services.AddLevelModule();

                    // Database
                    services.AddDbContext<BotDbContext>();
                    services.AddCommandEfCoreRepositories<BotDbContext>();
                    services.AddScoped<IUserWarningRepository, UserWarningRepository>();
                    services.AddScoped<ISettingRepository, SettingRepository>();
                    services.AddScoped<IUserExperienceRepository, UserExperienceRepository>();

                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                    });

                    services.Configure<LocalizationOptions>(options =>
                    {
                        options.Cultures.Add(new CultureInfo("nl-NL"));
                    });
                })
                .ConfigureOptions(builder =>
                {
                    builder.AddEnvironmentVariables();
                })
                .Configure(builder =>
                {
                    builder.Use((context, func) =>
                    {
                        CultureInfo.CurrentCulture = new CultureInfo("nl-NL");
                        CultureInfo.CurrentUICulture = new CultureInfo("nl-NL");
                        return func();
                    });

                    builder.UseIgnoreBots();
                    builder.UsePendingCommand();
                    builder.UsePrefix(">");
                    builder.UseCommands();
                })
                .Build()
                .RunAsync();
        }
    }
}
