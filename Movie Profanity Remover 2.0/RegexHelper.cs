﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Movie_Profanity_Remover_2._0
{
    public static class RegexHelper
    {
        /// <summary>
        /// Replaces tokens in a pattern with values from a file path.
        /// Tokens are in the format {TokenName}.
        /// </summary>
        /// <param name="pattern">The pattern containing tokens to replace.</param>
        /// <param name="filePath">The file path to extract values from.</param>
        /// <returns>The pattern with tokens replaced by their corresponding values.</returns>
        public static string ReplaceTokens(string pattern, string filePath)
        {
            if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(filePath))
                return pattern;

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                DirectoryInfo dirInfo = fileInfo.Directory;

                // Create a dictionary of token replacements
                var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    // FileInfo properties
                    { "Name", RegexEscape(fileInfo.Name) },
                    { "FileName", RegexEscape(fileInfo.Name) }, // Alias for Name for backward compatibility
                    { "BaseName", RegexEscape(Path.GetFileNameWithoutExtension(fileInfo.Name)) },
                    { "Extension", RegexEscape(fileInfo.Extension) },
                    { "FullName", RegexEscape(fileInfo.FullName) },
                    { "DirectoryName", RegexEscape(fileInfo.DirectoryName) },
                    { "Length", fileInfo.Length.ToString() },
                    { "CreationTime", fileInfo.CreationTime.ToString("yyyy-MM-dd_HH-mm-ss") },
                    { "LastWriteTime", fileInfo.LastWriteTime.ToString("yyyy-MM-dd_HH-mm-ss") },
                    { "LastAccessTime", fileInfo.LastAccessTime.ToString("yyyy-MM-dd_HH-mm-ss") },
                    
                    // DirectoryInfo properties
                    { "DirectoryName", RegexEscape(dirInfo.Name) },
                    { "DirectoryFullName", RegexEscape(dirInfo.FullName) },
                    { "ParentDirectory", dirInfo.Parent != null ? RegexEscape(dirInfo.Parent.Name) : string.Empty },
                    { "ParentDirectoryFullName", dirInfo.Parent != null ? RegexEscape(dirInfo.Parent.FullName) : string.Empty },
                    
                    // Path components
                    { "Drive", RegexEscape(Path.GetPathRoot(fileInfo.FullName)) },
                    { "PathWithoutExtension", RegexEscape(Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(fileInfo.Name))) },
                    { "RelativePath", RegexEscape(GetRelativePath(fileInfo.FullName, Directory.GetCurrentDirectory())) }
                };

                // Replace all tokens in the pattern
                foreach (var replacement in replacements)
                {
                    pattern = Regex.Replace(
                        pattern, 
                        "\\{" + replacement.Key + "\\}", 
                        replacement.Value, 
                        RegexOptions.IgnoreCase
                    );
                }

                return pattern;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error replacing tokens: {ex.Message}");
                return pattern;
            }
        }

        /// <summary>
        /// Escapes a string for use in a regular expression pattern.
        /// </summary>
        /// <param name="input">The input string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string RegexEscape(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return Regex.Escape(input);
        }

        /// <summary>
        /// Gets the relative path from a base directory to a target path.
        /// </summary>
        /// <param name="targetPath">The target path.</param>
        /// <param name="baseDirectory">The base directory.</param>
        /// <returns>The relative path.</returns>
        private static string GetRelativePath(string targetPath, string baseDirectory)
        {
            if (string.IsNullOrEmpty(targetPath) || string.IsNullOrEmpty(baseDirectory))
                return targetPath;

            try
            {
                Uri baseUri = new Uri(baseDirectory.EndsWith("\\") ? baseDirectory : baseDirectory + "\\");
                Uri targetUri = new Uri(targetPath);

                Uri relativeUri = baseUri.MakeRelativeUri(targetUri);
                string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                return relativePath.Replace('/', '\\');
            }
            catch
            {
                return targetPath;
            }
        }

        /// <summary>
        /// Checks if a file matches a pattern with token replacement.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <param name="pattern">The pattern to match against.</param>
        /// <param name="referenceFilePath">The reference file path for token replacement.</param>
        /// <returns>True if the file matches the pattern, false otherwise.</returns>
        public static bool IsMatch(string filePath, string pattern, string referenceFilePath)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(pattern))
                return false;

            try
            {
                string resolvedPattern = ReplaceTokens(pattern, referenceFilePath);
                return Regex.IsMatch(
                    Path.GetFileName(filePath), 
                    resolvedPattern, 
                    RegexOptions.IgnoreCase
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error matching pattern: {ex.Message}");
                return false;
            }
        }
    }
}
