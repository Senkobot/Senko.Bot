using System.Linq;

namespace Senko.Modules.Levels.Utility
{
    public static class LevelCalculator
    {
        private static readonly long[] Levels = Enumerable.Repeat(0, MaxLevel + 1).Select((_, i) => CalculateExperience(i)).ToArray();
        
        public const int MaxLevel = 100;

        private static long CalculateExperience(long level)
        {
            var experience = 5d / 6 * level * (2 * level * level + 27 * level + 91);

            return (long)experience;
        }

        /// <summary>
        ///     Get the amount of experience that the level contains.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static long GetExperience(long level)
        {
            if (level >= MaxLevel)
            {
                level = MaxLevel;
            }

            return Levels[level];
        }

        /// <summary>
        ///     Get the level from the experience.
        /// </summary>
        /// <param name="experience">The experience.</param>
        /// <returns>The level.</returns>
        public static long GetLevel(long experience)
        {
            for (var i = 0; i < MaxLevel; i++)
            {
                if (experience < Levels[i])
                {
                    return i - 1;
                }
            }

            return MaxLevel;
        }
    }
}
