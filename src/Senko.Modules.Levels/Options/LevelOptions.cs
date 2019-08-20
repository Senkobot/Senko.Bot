using Senko.Framework;

namespace Senko.Modules.Levels
{
    [Configuration("Modules:Level")]
    public class LevelOptions
    {
        public bool EnableLeveling { get; set; } = true;
    }
}
