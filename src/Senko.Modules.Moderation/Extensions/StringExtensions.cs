namespace Senko.Modules.Moderation
{
    public static class StringExtensions
    {
        public static string MaxLength(this string source, int maxLength, string suffix = "")
        {
            if (source.Length <= maxLength)
            {
                return source;
            }

            return source.Substring(0, maxLength) + suffix;
        }
    }
}
