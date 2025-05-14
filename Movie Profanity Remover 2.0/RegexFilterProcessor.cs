﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Movie_Profanity_Remover_2._0
{
    /// <summary>
    /// Processes text using regex patterns for filtering profanity.
    /// </summary>
    public class RegexFilterProcessor
    {
        private List<Regex> _includePatterns = new List<Regex>();

        /// <summary>
        /// Initializes a new instance of the RegexFilterProcessor class.
        /// </summary>
        /// <param name="includePatterns">List of regex patterns to include in filtering.</param>
        public RegexFilterProcessor(List<string> includePatterns)
        {
            if (includePatterns != null)
            {
                foreach (var pattern in includePatterns)
                {
                    try
                    {
                        _includePatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating regex pattern '{pattern}': {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the text contains any matches according to the include patterns.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>True if the text contains matches, false otherwise.</returns>
        public bool ContainsMatch(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Check include patterns
            foreach (var pattern in _includePatterns)
            {
                if (pattern.IsMatch(text))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds all matches in the text according to the include patterns.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>A list of matches found in the text.</returns>
        public List<Match> FindMatches(string text)
        {
            var matches = new List<Match>();

            if (string.IsNullOrWhiteSpace(text))
                return matches;

            // Check include patterns
            foreach (var pattern in _includePatterns)
            {
                var patternMatches = pattern.Matches(text);
                foreach (Match match in patternMatches)
                {
                    if (match.Success)
                    {
                        matches.Add(match);
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// Censors the text by replacing all matches with asterisks.
        /// </summary>
        /// <param name="text">The text to censor.</param>
        /// <returns>The censored text.</returns>
        public string CensorText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            string result = text;

            // Find all matches
            var matches = FindMatches(text);

            // Sort matches by index in descending order to avoid index shifting
            matches.Sort((a, b) => b.Index.CompareTo(a.Index));

            // Replace each match with asterisks
            foreach (var match in matches)
            {
                string censored = new string('*', match.Length);
                result = result.Substring(0, match.Index) + censored + result.Substring(match.Index + match.Length);
            }

            return result;
        }

        /// <summary>
        /// Creates a RegexFilterProcessor from the application settings.
        /// </summary>
        /// <returns>A RegexFilterProcessor instance.</returns>
        public static RegexFilterProcessor FromSettings()
        {
            return new RegexFilterProcessor(
                Tool.Settings.RegexIncludePatterns
            );
        }
    }
}
