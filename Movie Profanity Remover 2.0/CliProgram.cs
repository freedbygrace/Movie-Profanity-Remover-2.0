﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CommandLine;
using CommandLine.Text;

namespace Movie_Profanity_Remover_2._0
{
    /// <summary>
    /// A TextWriter that writes to multiple TextWriters at once.
    /// </summary>
    public class MultiTextWriter : TextWriter
    {
        private readonly IEnumerable<TextWriter> _writers;

        public MultiTextWriter(IEnumerable<TextWriter> writers)
        {
            _writers = writers;
        }

        public override void Write(char value)
        {
            foreach (var writer in _writers)
                writer.Write(value);
        }

        public override void Write(string value)
        {
            foreach (var writer in _writers)
                writer.Write(value);
        }

        public override void WriteLine(string value)
        {
            foreach (var writer in _writers)
                writer.WriteLine(value);
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
    }
    public class CliOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input directory containing video files to process.")]
        public string InputDirectory { get; set; }

        [Option('r', "recursive", Default = false, HelpText = "Search for video files recursively in subdirectories.")]
        public bool Recursive { get; set; }

        [Option('d', "depth", Default = -1, HelpText = "Maximum recursion depth when searching for video files (-1 for unlimited).")]
        public int MaxDepth { get; set; }

        [Option('p', "pattern", Default = "({BaseName}).*\\.(srt)", HelpText = "Regex pattern for matching subtitle files to video files. Supports tokens like {Name}, {BaseName}, {Extension}, etc.")]
        public string SubtitlePattern { get; set; }

        [Option('o', "output", HelpText = "Output directory for processed files (defaults to same as input).")]
        public string OutputDirectory { get; set; }

        [Option('c', "config", HelpText = "Path to a configuration file with processing settings.")]
        public string ConfigFile { get; set; }

        [Option("create-subtitles", Default = true, HelpText = "Create new subtitle files with censored text.")]
        public bool CreateSubtitles { get; set; }

        [Option("embed-subtitles", Default = true, HelpText = "Embed subtitles in the output video file.")]
        public bool EmbedSubtitles { get; set; }

        [Option("delete-originals", Default = false, HelpText = "Delete original video and subtitle files after processing.")]
        public bool DeleteOriginalFiles { get; set; }

        [Option("mute-vocals-only", Default = false, HelpText = "Attempt to mute only vocal channels, preserving background audio.")]
        public bool MuteVocalsOnly { get; set; }

        [Option("subtitles-type", Default = "normal", HelpText = "Type of subtitles to create: normal, exclusive, or both.")]
        public string SubtitlesType { get; set; }

        [Option("output-type", Default = "mp4", HelpText = "Output video file format (mp4, mkv, etc.).")]
        public string OutputType { get; set; }

        [Option("use-regex", Default = false, HelpText = "Enable regex pattern matching for profanity filtering.")]
        public bool UseRegexFiltering { get; set; }

        [Option("regex-config", HelpText = "Path to a JSON file containing regex filter configuration.")]
        public string RegexConfigFile { get; set; }

        [Option("save-regex-config", HelpText = "Path to save the current regex filter configuration as a JSON file.")]
        public string SaveRegexConfigFile { get; set; }

        [Option("create-default-config", HelpText = "Path to save a default regex filter configuration as a JSON file.")]
        public string CreateDefaultConfigFile { get; set; }

        [Option("custom-affix", Default = "_SL", HelpText = "Custom affix to add to output filenames.")]
        public string CustomAffix { get; set; }

        [Option("aspect-ratio", Default = false, HelpText = "Apply custom aspect ratio to output video.")]
        public bool AspectRatio { get; set; }

        [Option("aspect-width", Default = 16, HelpText = "Aspect ratio width.")]
        public int AspectRatioWidth { get; set; }

        [Option("aspect-height", Default = 9, HelpText = "Aspect ratio height.")]
        public int AspectRatioHeight { get; set; }

        [Option("single-word", Default = false, HelpText = "Enable single word filtering.")]
        public bool SingleWord { get; set; }

        [Option("single-word-before", Default = 500, HelpText = "Milliseconds to mute before a single profanity word.")]
        public int SingleWordBefore { get; set; }

        [Option("single-word-after", Default = 500, HelpText = "Milliseconds to mute after a single profanity word.")]
        public int SingleWordAfter { get; set; }

        [Option("list-tokens", HelpText = "List all available tokens for subtitle pattern matching.")]
        public bool ListTokens { get; set; }

        [Option("verbose", Default = false, HelpText = "Enable verbose output.")]
        public bool Verbose { get; set; }

        [Option("dry-run", Default = false, HelpText = "Perform a dry run without actually processing videos.")]
        public bool DryRun { get; set; }

        [Option("log-file", HelpText = "Path to a log file to write output.")]
        public string LogFile { get; set; }
    }

    public static class CliProgram
    {
        private static TextWriter _originalConsoleOut;
        private static StreamWriter _logWriter;

        public static void Run(string[] args)
        {
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<CliOptions>(args);

            parserResult
                .WithParsed(options => ProcessVideos(options))
                .WithNotParsed(errors => DisplayHelp(parserResult, errors));
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errors)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = "Movie Profanity Remover 2.0 CLI";
                h.Copyright = $"Copyright (c) {DateTime.Now.Year}";
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }

        private static void ProcessVideos(CliOptions options)
        {
            try
            {
                // Set up logging if requested
                SetupLogging(options);

                Console.WriteLine("Movie Profanity Remover 2.0 CLI");
                Console.WriteLine("======================================");
                Console.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");
                Console.WriteLine($"Date: {DateTime.Now}");
                Console.WriteLine("======================================");

                // Check if we should just create a default config file
                if (!string.IsNullOrEmpty(options.CreateDefaultConfigFile))
                {
                    var defaultConfig = RegexFilterConfig.CreateDefault();
                    defaultConfig.SaveToJson(options.CreateDefaultConfigFile);
                    Console.WriteLine($"Created default regex filter configuration at: {options.CreateDefaultConfigFile}");
                    return;
                }

                // Check if we should just list tokens
                if (options.ListTokens)
                {
                    ListAvailableTokens();
                    return;
                }

                // Initialize settings
                Tool.LoadSettings();

                // Load regex configuration from JSON file if specified
                if (!string.IsNullOrEmpty(options.RegexConfigFile))
                {
                    LoadRegexConfigFromJson(options.RegexConfigFile);
                }
                else if (options.UseRegexFiltering)
                {
                    Console.WriteLine("Warning: Regex filtering is enabled but no configuration file is specified.");
                    Console.WriteLine("Please use --regex-config to specify a configuration file.");
                    Console.WriteLine("Creating and using a default configuration...");

                    // Create a default configuration
                    Tool.Settings.RegexIncludePatterns = RegexFilterConfig.CreateDefault().IncludePatterns;
                    Tool.Settings.UseRegexFiltering = true;
                }

                // Apply CLI options to settings (this will override any loaded configuration)
                ApplyCliOptionsToSettings(options);

                // Save regex configuration to JSON file if specified
                if (!string.IsNullOrEmpty(options.SaveRegexConfigFile))
                {
                    SaveRegexConfigToJson(options.SaveRegexConfigFile);
                }

                // Create batch processor and process videos
                if (!options.DryRun)
                {
                    var batchProcessor = new BatchProcessor();
                    batchProcessor.ProcessDirectory(options);
                }
                else
                {
                    Console.WriteLine("Dry run mode - no videos will be processed.");
                    Console.WriteLine("Input directory: " + options.InputDirectory);
                    Console.WriteLine("Recursive: " + options.Recursive);
                    Console.WriteLine("Max depth: " + options.MaxDepth);
                    Console.WriteLine("Subtitle pattern: " + options.SubtitlePattern);
                    Console.WriteLine("Output directory: " + (string.IsNullOrEmpty(options.OutputDirectory) ? "(same as input)" : options.OutputDirectory));
                    Console.WriteLine("Output type: " + options.OutputType);
                    Console.WriteLine("Create subtitles: " + options.CreateSubtitles);
                    Console.WriteLine("Embed subtitles: " + options.EmbedSubtitles);
                    Console.WriteLine("Delete originals: " + options.DeleteOriginalFiles);
                    Console.WriteLine("Mute vocals only: " + options.MuteVocalsOnly);
                    Console.WriteLine("Subtitles type: " + options.SubtitlesType);
                    Console.WriteLine("Custom affix: " + options.CustomAffix);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (options.Verbose)
                {
                    Console.WriteLine(ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        Console.WriteLine(ex.InnerException.StackTrace);
                    }
                }
                Environment.Exit(1);
            }
            finally
            {
                CleanupLogging();
            }
        }

        private static void SetupLogging(CliOptions options)
        {
            if (!string.IsNullOrEmpty(options.LogFile))
            {
                try
                {
                    _originalConsoleOut = Console.Out;
                    _logWriter = new StreamWriter(options.LogFile, true);
                    _logWriter.AutoFlush = true;

                    // Create a writer that writes to both console and log file
                    Console.SetOut(new MultiTextWriter(new[] { _originalConsoleOut, _logWriter }));

                    Console.WriteLine($"Logging to file: {options.LogFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting up logging: {ex.Message}");
                    _logWriter = null;
                }
            }
        }

        private static void CleanupLogging()
        {
            if (_logWriter != null)
            {
                Console.SetOut(_originalConsoleOut);
                _logWriter.Close();
                _logWriter = null;
            }
        }

        private static void ListAvailableTokens()
        {
            Console.WriteLine("Available tokens for subtitle pattern matching:");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("{Name} - The filename with extension (e.g., 'movie.mp4')");
            Console.WriteLine("{FileName} - Alias for Name");
            Console.WriteLine("{BaseName} - The filename without extension (e.g., 'movie')");
            Console.WriteLine("{Extension} - The file extension (e.g., '.mp4')");
            Console.WriteLine("{FullName} - The full path to the file");
            Console.WriteLine("{DirectoryName} - The name of the directory containing the file");
            Console.WriteLine("{Length} - The file size in bytes");
            Console.WriteLine("{CreationTime} - The file creation time (format: yyyy-MM-dd_HH-mm-ss)");
            Console.WriteLine("{LastWriteTime} - The file last write time (format: yyyy-MM-dd_HH-mm-ss)");
            Console.WriteLine("{LastAccessTime} - The file last access time (format: yyyy-MM-dd_HH-mm-ss)");
            Console.WriteLine("{DirectoryName} - The name of the directory containing the file");
            Console.WriteLine("{DirectoryFullName} - The full path to the directory containing the file");
            Console.WriteLine("{ParentDirectory} - The name of the parent directory");
            Console.WriteLine("{ParentDirectoryFullName} - The full path to the parent directory");
            Console.WriteLine("{Drive} - The drive letter or root path");
            Console.WriteLine("{PathWithoutExtension} - The full path without the file extension");
            Console.WriteLine("{RelativePath} - The path relative to the current directory");
            Console.WriteLine();
            Console.WriteLine("Example patterns:");
            Console.WriteLine("  \"{BaseName}.*\\.srt\" - Match any .srt file with the same base name as the video");
            Console.WriteLine("  \"{BaseName}\\.eng\\.srt\" - Match English subtitle files (movie.eng.srt)");
            Console.WriteLine("  \"{BaseName}(_.*)?\\.(srt|sub|idx)\" - Match multiple subtitle formats with optional suffix");
        }

        private static void ApplyCliOptionsToSettings(CliOptions options)
        {
            // Apply CLI options to the global settings
            Tool.Settings.CreateSubtitles = options.CreateSubtitles;
            Tool.Settings.EmbedSubtitles = options.EmbedSubtitles;
            Tool.Settings.DeleteOriginalFiles = options.DeleteOriginalFiles;
            Tool.Settings.MuteVocalsOnly = options.MuteVocalsOnly;
            Tool.Settings.OutputType = options.OutputType;
            Tool.Settings.CustomAffix = options.CustomAffix;

            // Apply aspect ratio settings
            Tool.Settings.AspectRatio = options.AspectRatio;
            Tool.Settings.AspectRatioWidth = options.AspectRatioWidth;
            Tool.Settings.AspectRatioHeight = options.AspectRatioHeight;

            // Apply single word settings
            Tool.Settings.SingleWord = options.SingleWord;
            Tool.Settings.SingleWordBefore = options.SingleWordBefore;
            Tool.Settings.SingleWordAfter = options.SingleWordAfter;

            // Apply regex filtering settings
            Tool.Settings.UseRegexFiltering = options.UseRegexFiltering;

            // If regex filtering is enabled but no patterns are loaded, warn the user
            if (options.UseRegexFiltering &&
                (Tool.Settings.RegexIncludePatterns == null || Tool.Settings.RegexIncludePatterns.Count == 0) &&
                string.IsNullOrEmpty(options.RegexConfigFile))
            {
                Console.WriteLine("Warning: Regex filtering is enabled but no patterns are specified.");
                Console.WriteLine("Please use --regex-config to specify a configuration file.");
            }

            // Set subtitle type
            switch (options.SubtitlesType.ToLower())
            {
                case "normal":
                    Tool.Settings.NormalSubtitles = true;
                    Tool.Settings.ExclusiveSubtitles = false;
                    Tool.Settings.BothSubtitles = false;
                    break;
                case "exclusive":
                    Tool.Settings.NormalSubtitles = false;
                    Tool.Settings.ExclusiveSubtitles = true;
                    Tool.Settings.BothSubtitles = false;
                    break;
                case "both":
                    Tool.Settings.NormalSubtitles = false;
                    Tool.Settings.ExclusiveSubtitles = false;
                    Tool.Settings.BothSubtitles = true;
                    break;
                default:
                    Console.WriteLine($"Warning: Unknown subtitles type '{options.SubtitlesType}'. Using 'normal' instead.");
                    Tool.Settings.NormalSubtitles = true;
                    Tool.Settings.ExclusiveSubtitles = false;
                    Tool.Settings.BothSubtitles = false;
                    break;
            }

            // Load filter words if specified
            if (!string.IsNullOrEmpty(options.FilterFile) && File.Exists(options.FilterFile))
            {
                LoadFilterWordsFromFile(options.FilterFile);
            }

            // Save settings
            Tool.SaveSettings();

            if (options.Verbose)
            {
                Console.WriteLine("Applied settings:");
                Console.WriteLine($"  Create subtitles: {Tool.Settings.CreateSubtitles}");
                Console.WriteLine($"  Embed subtitles: {Tool.Settings.EmbedSubtitles}");
                Console.WriteLine($"  Delete original files: {Tool.Settings.DeleteOriginalFiles}");
                Console.WriteLine($"  Mute vocals only: {Tool.Settings.MuteVocalsOnly}");
                Console.WriteLine($"  Output type: {Tool.Settings.OutputType}");
                Console.WriteLine($"  Custom affix: {Tool.Settings.CustomAffix}");
                Console.WriteLine($"  Aspect ratio: {Tool.Settings.AspectRatio}");
                if (Tool.Settings.AspectRatio)
                {
                    Console.WriteLine($"  Aspect ratio dimensions: {Tool.Settings.AspectRatioWidth}:{Tool.Settings.AspectRatioHeight}");
                }
                Console.WriteLine($"  Single word: {Tool.Settings.SingleWord}");
                if (Tool.Settings.SingleWord)
                {
                    Console.WriteLine($"  Single word before: {Tool.Settings.SingleWordBefore}ms");
                    Console.WriteLine($"  Single word after: {Tool.Settings.SingleWordAfter}ms");
                }

                string subtitlesType = "normal";
                if (Tool.Settings.ExclusiveSubtitles) subtitlesType = "exclusive";
                if (Tool.Settings.BothSubtitles) subtitlesType = "both";
                Console.WriteLine($"  Subtitles type: {subtitlesType}");

                // Display regex filtering settings
                Console.WriteLine($"  Regex filtering: {Tool.Settings.UseRegexFiltering}");
                if (Tool.Settings.UseRegexFiltering)
                {
                    if (Tool.Settings.RegexIncludePatterns != null && Tool.Settings.RegexIncludePatterns.Count > 0)
                    {
                        Console.WriteLine($"  Regex include patterns: {Tool.Settings.RegexIncludePatterns.Count}");
                        foreach (var pattern in Tool.Settings.RegexIncludePatterns)
                        {
                            Console.WriteLine($"    {pattern}");
                        }
                    }

                    if (Tool.Settings.RegexExcludePatterns != null && Tool.Settings.RegexExcludePatterns.Count > 0)
                    {
                        Console.WriteLine($"  Regex exclude patterns: {Tool.Settings.RegexExcludePatterns.Count}");
                        foreach (var pattern in Tool.Settings.RegexExcludePatterns)
                        {
                            Console.WriteLine($"    {pattern}");
                        }
                    }
                }

                // Display custom filter words
                if (Tool.Settings.WordFullCustom != null && Tool.Settings.WordFullCustom.Count > 0)
                {
                    Console.WriteLine($"  Custom filter words: {Tool.Settings.WordFullCustom.Count}");
                    if (Tool.Settings.WordFullCustom.Count <= 10)
                    {
                        Console.WriteLine($"    {string.Join(", ", Tool.Settings.WordFullCustom)}");
                    }
                    else
                    {
                        Console.WriteLine($"    {string.Join(", ", Tool.Settings.WordFullCustom.Take(10))}...");
                    }
                }
            }
        }



        private static void LoadRegexConfigFromJson(string filePath)
        {
            try
            {
                Console.WriteLine($"Loading regex configuration from JSON file: {filePath}");

                // Load the configuration from the JSON file
                var config = RegexFilterConfig.LoadFromJson(filePath);

                // Create a filter from the configuration (this will handle AssumeWordBoundary)
                var filter = config.CreateFilter();

                // Apply the filter patterns to the settings
                Tool.Settings.RegexIncludePatterns = new List<string>();
                foreach (var pattern in filter.IncludePatterns)
                {
                    Tool.Settings.RegexIncludePatterns.Add(pattern.ToString());
                }

                Tool.Settings.UseRegexFiltering = true;

                Console.WriteLine($"Applied regex configuration '{config.Name}' to settings");
                Console.WriteLine($"Loaded {filter.IncludePatterns.Count} regex patterns");

                if (filter.IncludePatterns.Count > 0 && filter.IncludePatterns.Count <= 10)
                {
                    Console.WriteLine("Patterns:");
                    foreach (var pattern in filter.IncludePatterns)
                    {
                        Console.WriteLine($"  - {pattern}");
                    }
                }
                else if (filter.IncludePatterns.Count > 10)
                {
                    Console.WriteLine("First 10 patterns:");
                    for (int i = 0; i < 10; i++)
                    {
                        Console.WriteLine($"  - {filter.IncludePatterns[i]}");
                    }
                    Console.WriteLine($"  ... and {filter.IncludePatterns.Count - 10} more");
                }

                Console.WriteLine($"AssumeWordBoundary: {config.AssumeWordBoundary}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading regex configuration from JSON: {ex.Message}");
            }
        }

        private static void SaveRegexConfigToJson(string filePath)
        {
            try
            {
                Console.WriteLine($"Saving current regex configuration to JSON file: {filePath}");

                // Check if we have any patterns to save
                bool hasPatterns =
                    (Tool.Settings.RegexIncludePatterns != null && Tool.Settings.RegexIncludePatterns.Count > 0);

                if (!hasPatterns)
                {
                    Console.WriteLine("Warning: No regex patterns are currently loaded.");
                    Console.WriteLine("Creating a default configuration instead.");

                    // Create and save a default configuration
                    var defaultConfig = RegexFilterConfig.CreateDefault();
                    defaultConfig.SaveToJson(filePath);

                    Console.WriteLine($"Saved default regex configuration to: {filePath}");
                    return;
                }

                // Create a configuration from the current settings
                var config = new RegexFilterConfig
                {
                    Name = "Custom Configuration",
                    Description = "Configuration created from application settings",
                    IncludePatterns = new List<string>(Tool.Settings.RegexIncludePatterns ?? new List<string>()),
                    AssumeWordBoundary = true
                };

                // Save the configuration to the JSON file
                config.SaveToJson(filePath);

                Console.WriteLine($"Saved regex configuration to: {filePath}");
                Console.WriteLine($"  - Include patterns: {config.IncludePatterns.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving regex configuration to JSON: {ex.Message}");
            }
        }
    }
}
