using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Senko.Common.Services;
using Senko.Modules.Levels.Managers;

namespace Senko.Modules.Levels.Services
{
    public class StoreLevelService : TimedHostedService
    {
        private readonly LevelManager _manager;

        public StoreLevelService(ILogger<StoreLevelService> logger, LevelManager manager) : base(logger)
        {
            _manager = manager;
        }

        public override TimeSpan Period => TimeSpan.FromSeconds(5);

        public override Task DoWorkAsync() => _manager.StoreQueueAsync();
    }
}
