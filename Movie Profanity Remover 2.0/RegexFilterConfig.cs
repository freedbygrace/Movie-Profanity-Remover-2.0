﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Movie_Profanity_Remover_2._0
{
    /// <summary>
    /// Represents a configuration for regex filtering with include and exclude patterns.
    /// </summary>
    [Serializable]
    public class RegexFilterConfig
    {
        /// <summary>
        /// Gets or sets the name of the configuration.
        /// </summary>
        public string Name { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the description of the configuration.
        /// </summary>
        public string Description { get; set; } = "Default regex filter configuration";

        /// <summary>
        /// Gets or sets the list of regex patterns to include.
        /// </summary>
        public List<string> IncludePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets whether to enable case-sensitive matching.
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to automatically add word boundaries to simple patterns.
        /// </summary>
        public bool AssumeWordBoundary { get; set; } = false;

        /// <summary>
        /// Creates a SwearWordFilter from this configuration.
        /// </summary>
        /// <returns>A SwearWordFilter instance.</returns>
        public SwearWordFilter CreateFilter()
        {
            var filter = new SwearWordFilter();

            // Add include patterns
            foreach (var pattern in IncludePatterns)
            {
                if (AssumeWordBoundary)
                {
                    // Check if the pattern already has word boundaries
                    if (pattern.StartsWith("\\b") && pattern.EndsWith("\\b"))
                    {
                        // Pattern already has word boundaries, use as is
                        filter.AddIncludePattern(pattern);
                    }
                    else if (!pattern.Contains("\\b"))
                    {
                        // Pattern doesn't have word boundaries, add them
                        filter.AddIncludePattern($"\\b{pattern}\\b");
                    }
                    else
                    {
                        // Pattern has some word boundaries but not at both ends, use as is
                        filter.AddIncludePattern(pattern);
                    }
                }
                else
                {
                    // Use pattern as is
                    filter.AddIncludePattern(pattern);
                }
            }

            return filter;
        }

        /// <summary>
        /// Loads a RegexFilterConfig from a JSON file.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>A RegexFilterConfig instance.</returns>
        public static RegexFilterConfig LoadFromJson(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Filter configuration file not found: {filePath}");
                    return new RegexFilterConfig();
                }

                string json = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<RegexFilterConfig>(json);

                if (config == null)
                {
                    Console.WriteLine($"Failed to parse filter configuration file: {filePath}");
                    return new RegexFilterConfig();
                }

                Console.WriteLine($"Loaded filter configuration '{config.Name}' from {filePath}");
                Console.WriteLine($"Description: {config.Description}");
                Console.WriteLine($"Include patterns: {config.IncludePatterns.Count}");

                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading filter configuration: {ex.Message}");
                return new RegexFilterConfig();
            }
        }

        /// <summary>
        /// Saves this RegexFilterConfig to a JSON file.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool SaveToJson(string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filePath, json);
                Console.WriteLine($"Saved filter configuration '{Name}' to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving filter configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates a default RegexFilterConfig with common profanity patterns.
        /// </summary>
        /// <returns>A RegexFilterConfig instance with default patterns.</returns>
        public static RegexFilterConfig CreateDefault()
        {
            var config = new RegexFilterConfig
            {
                Name = "Standard Profanity Filter",
                Description = "Standard configuration with common profanity patterns using (^.*WORD.*$) format",
                AssumeWordBoundary = false
            };

            // Add common profanity words using the (^.*WORD.*$) format
            config.IncludePatterns.AddRange(new[]
            {
                "(^.*ass.*$)",
                "(^.*fuck.*$)",
                "(^.*shit.*$)",
                "(^.*bitch.*$)",
                "(^.*damn.*$)",
                "(^.*hell.*$)",
                "(^.*cunt.*$)",
                "(^.*dick.*$)",
                "(^.*cock.*$)",
                "(^.*bastard.*$)",
                "(^.*asshole.*$)",
                "(^.*motherfucker.*$)",
                "(^.*bullshit.*$)",
                "(^.*goddamn.*$)",
                "(^.*piss.*$)",
                "(^.*pussy.*$)",
                "(^.*whore.*$)",
                "(^.*slut.*$)",
                "(^.*tits.*$)"
            });

            return config;
        }
    }
}
