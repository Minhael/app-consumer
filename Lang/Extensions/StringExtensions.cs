using System.Diagnostics;

namespace Common.Lang.Extensions
{
    [DebuggerStepThrough]
    public static class StringExtensions
    {
        /// <summary>
        /// Remove the first character in string.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string Pop(this string self)
        {
            return self.Remove(self.Length - 1, 1);
        }

        /// <summary>
        /// Substring first n characters from string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);
            return (value.Length <= maxLength ? value : value.Substring(0, maxLength));
        }

        /// <summary>
        /// Return true if string is start with one of the prefix; otherwise false.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="comparisonType"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool StartWith(this string self, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase, params String[] values)
        {
            return values.Any(x => self.StartsWith(x, comparisonType));
        }

        /// <summary>
        /// Shorthand of string.IsNullOrWhiteSpace
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string self)
        {
            return string.IsNullOrWhiteSpace(self);
        }

        /// <summary>
        /// Shorthand of string.Format
        /// </summary>
        /// <param name="self"></param>
        /// <param name="argv"></param>
        /// <returns></returns>
        public static string Format(this string self, params object?[] argv)
        {
            return string.Format(self, argv);
        }

        /// <summary>
        /// Return the first non null string.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string? Coalesc(this string? self, params string?[] values)
        {
            return self.Coalesc(values, s => s?.IsNullOrWhiteSpace() == false);
        }

        /// <summary>
        /// Trim the prefix from string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string StripPrefix(this string text, string prefix)
        {
            return text.StartsWith(prefix) ? text.Substring(prefix.Length) : text;
        }

        /// <summary>
        /// Parse a period syntax to pair of from, to in int.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static (int from, int to) ParseAsPeriod(this string self, string separator = "..")
        {
            var (from, to, _) = self.Split(separator, 2);
            return (Int32.Parse(from), Int32.Parse(to));
        }

        public static (long from, long to) ParseAsPeriodLong(this string self, string separator = "..")
        {
            var (from, to, _) = self.Split(separator, 2);
            return (Int64.Parse(from), Int64.Parse(to));
        }
    }
}