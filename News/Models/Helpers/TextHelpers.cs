using System;

namespace News.Models.Helpers // Use your project's namespace
{
    public static class TextHelpers
    {
        private const int WordsPerMinute = 225; // Average reading speed

        /// <summary>
        /// Calculates the estimated reading time in minutes for a given text.
        /// </summary>
        /// <param name="text">The text content to analyze.</param>
        /// <returns>The estimated reading time in minutes.</returns>
        public static int CalculateReadingTime(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            // Count the words, splitting by spaces and newlines
            var wordCount = text.Split(new[] { ' ', '\r', '\n' },
                                    StringSplitOptions.RemoveEmptyEntries).Length;

            if (wordCount == 0) return 0;

            // Calculate minutes and round up to the nearest whole number
            var minutes = Math.Ceiling((double)wordCount / WordsPerMinute);

            return Convert.ToInt32(minutes);
        }
    }
}