﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Movie_Profanity_Remover_2._0
{
    /// <summary>
    /// Represents a swear word filter that uses regex patterns.
    /// </summary>
    public class SwearWordFilter
    {
        /// <summary>
        /// Gets the list of regex patterns to include in filtering.
        /// </summary>
        public List<Regex> IncludePatterns { get; private set; } = new List<Regex>();

        /// <summary>
        /// Adds a regex pattern to include in filtering.
        /// </summary>
        /// <param name="pattern">The regex pattern to include.</param>
        public void AddIncludePattern(string pattern)
        {
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                try
                {
                    IncludePatterns.Add(new Regex(pattern.Trim(), RegexOptions.IgnoreCase));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding include pattern '{pattern}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Checks if the given text contains any matches according to the filter rules.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>True if the text contains matches, false otherwise.</returns>
        public bool ContainsMatch(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Check include patterns
            foreach (var pattern in IncludePatterns)
            {
                if (pattern.IsMatch(text))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds all matches in the given text according to the filter rules.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>A list of matches found in the text.</returns>
        public List<Match> FindMatches(string text)
        {
            var matches = new List<Match>();

            if (string.IsNullOrWhiteSpace(text))
                return matches;

            // Check include patterns
            foreach (var pattern in IncludePatterns)
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


    }
}
