﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Movie_Profanity_Remover_2._0
{
    public class BatchProcessor
    {
        private CancellationTokenSource _cancellationTokenSource;
        private List<Video> _videos = new List<Video>();
        private readonly string[] _supportedVideoExtensions = { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm" };
        private CliOptions _options;
        private int _totalVideos;
        private int _processedVideos;
        private int _successfulVideos;

        public void ProcessDirectory(CliOptions options)
        {
            _options = options;
            _totalVideos = 0;
            _processedVideos = 0;
            _successfulVideos = 0;

            Console.WriteLine("Movie Profanity Remover 2.0 - CLI Mode");
            Console.WriteLine("======================================");
            Console.WriteLine($"Processing directory: {options.InputDirectory}");
            Console.WriteLine($"Recursive search: {options.Recursive}");

            if (options.Recursive && options.MaxDepth >= 0)
            {
                Console.WriteLine($"Maximum recursion depth: {options.MaxDepth}");
            }

            Console.WriteLine($"Subtitle pattern: {options.SubtitlePattern}");
            Console.WriteLine($"Output type: {options.OutputType}");
            Console.WriteLine($"Create subtitles: {options.CreateSubtitles}");
            Console.WriteLine($"Embed subtitles: {options.EmbedSubtitles}");
            Console.WriteLine($"Delete originals: {options.DeleteOriginalFiles}");
            Console.WriteLine($"Mute vocals only: {options.MuteVocalsOnly}");
            Console.WriteLine($"Subtitles type: {options.SubtitlesType}");
            Console.WriteLine("======================================");

            // Ensure the input directory exists
            if (!Directory.Exists(options.InputDirectory))
            {
                throw new DirectoryNotFoundException($"Input directory not found: {options.InputDirectory}");
            }

            // Create output directory if specified and doesn't exist
            if (!string.IsNullOrEmpty(options.OutputDirectory) && !Directory.Exists(options.OutputDirectory))
            {
                Console.WriteLine($"Creating output directory: {options.OutputDirectory}");
                Directory.CreateDirectory(options.OutputDirectory);
            }

            // Find video files
            var videoFiles = FindVideoFiles(options.InputDirectory, options.Recursive, options.MaxDepth);
            _totalVideos = videoFiles.Count;

            Console.WriteLine($"Found {videoFiles.Count} video files");

            if (videoFiles.Count == 0)
            {
                Console.WriteLine("No video files found. Exiting.");
                return;
            }

            // Match subtitle files to video files
            int matchedCount = MatchSubtitleFiles(videoFiles, options.SubtitlePattern);
            Console.WriteLine($"Found matching subtitles for {matchedCount} video files");

            if (matchedCount == 0)
            {
                Console.WriteLine("No matching subtitle files found. Exiting.");
                return;
            }

            // Process videos
            ProcessVideos();
        }

        private List<string> FindVideoFiles(string directory, bool recursive, int maxDepth)
        {
            var videoFiles = new List<string>();

            try
            {
                // Get all files in the current directory
                foreach (var file in Directory.GetFiles(directory))
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (_supportedVideoExtensions.Contains(extension))
                    {
                        videoFiles.Add(file);
                    }
                }

                // Recursively search subdirectories if requested
                if (recursive && (maxDepth < 0 || maxDepth > 0))
                {
                    foreach (var subDir in Directory.GetDirectories(directory))
                    {
                        int newDepth = maxDepth - 1;
                        videoFiles.AddRange(FindVideoFiles(subDir, true, newDepth));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching directory {directory}: {ex.Message}");
            }

            return videoFiles;
        }

        private int MatchSubtitleFiles(List<string> videoFiles, string subtitlePattern)
        {
            int matchedCount = 0;
            Console.WriteLine("Matching subtitle files to videos...");

            foreach (var videoFile in videoFiles)
            {
                string videoDirectory = Path.GetDirectoryName(videoFile);

                // Find matching subtitle files using the RegexHelper
                var subtitleFiles = FindMatchingSubtitleFiles(videoDirectory, subtitlePattern, videoFile);

                if (subtitleFiles.Count > 0)
                {
                    foreach (var subtitleFile in subtitleFiles)
                    {
                        // Create a video object and add it to the processing queue
                        var video = new Video
                        {
                            Input = videoFile,
                            SubtitlesPathOriginal = subtitleFile,
                            Output = GenerateOutputPath(videoFile)
                        };

                        _videos.Add(video);
                        matchedCount++;

                        Console.WriteLine($"Matched: {Path.GetFileName(videoFile)} with {Path.GetFileName(subtitleFile)}");
                    }
                }
                else
                {
                    Console.WriteLine($"No matching subtitle found for: {Path.GetFileName(videoFile)}");
                }
            }

            return matchedCount;
        }

        private List<string> FindMatchingSubtitleFiles(string directory, string pattern, string referenceFilePath)
        {
            var matchingFiles = new List<string>();

            try
            {
                // Get all subtitle files in the directory
                var subtitleFiles = Directory.GetFiles(directory)
                    .Where(f => Path.GetExtension(f).ToLower() == ".srt")
                    .ToList();

                // Log the pattern and available subtitle files for debugging
                Console.WriteLine($"  Looking for subtitles in {directory}");
                Console.WriteLine($"  Using pattern: {pattern}");
                Console.WriteLine($"  Found {subtitleFiles.Count} subtitle files to check");

                foreach (var file in subtitleFiles)
                {
                    // Use the RegexHelper to check if the file matches the pattern
                    if (RegexHelper.IsMatch(file, pattern, referenceFilePath))
                    {
                        matchingFiles.Add(file);
                        Console.WriteLine($"  Match found: {Path.GetFileName(file)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding matching subtitle files: {ex.Message}");
            }

            return matchingFiles;
        }

        private string GenerateOutputPath(string videoPath)
        {
            string outputDirectory;

            // Use the specified output directory if provided, otherwise use the same directory as the input
            if (!string.IsNullOrEmpty(_options.OutputDirectory))
            {
                outputDirectory = _options.OutputDirectory;

                // If the input is in a subdirectory and we're processing recursively,
                // preserve the directory structure in the output
                if (_options.Recursive)
                {
                    string inputRootDir = Path.GetFullPath(_options.InputDirectory);
                    string videoDir = Path.GetDirectoryName(Path.GetFullPath(videoPath));

                    if (videoDir.StartsWith(inputRootDir) && videoDir.Length > inputRootDir.Length)
                    {
                        // Get the relative path from the input root to the video directory
                        string relativePath = videoDir.Substring(inputRootDir.Length).TrimStart('\\', '/');
                        outputDirectory = Path.Combine(outputDirectory, relativePath);

                        // Create the directory if it doesn't exist
                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                        }
                    }
                }
            }
            else
            {
                outputDirectory = Path.GetDirectoryName(videoPath);
            }

            string fileName = Path.GetFileNameWithoutExtension(videoPath);
            string extension = "." + Tool.Settings.OutputType;

            return Path.Combine(outputDirectory, fileName + Tool.Settings.CustomAffix + extension);
        }

        private void ProcessVideos()
        {
            if (_videos.Count == 0)
            {
                Console.WriteLine("No videos to process. Exiting.");
                return;
            }

            Console.WriteLine($"Processing {_videos.Count} videos...");
            Console.WriteLine("======================================");

            _cancellationTokenSource = new CancellationTokenSource();
            FFMPEG.tokenSource = _cancellationTokenSource;

            // Set up console event handler for Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _cancellationTokenSource.Cancel();
                Console.WriteLine("\nProcessing cancelled by user.");
            };

            _successfulVideos = 0;
            _processedVideos = 0;
            _totalVideos = _videos.Count;

            // Start a task to update the progress bar
            var progressTask = Task.Run(() => UpdateProgressBar());

            for (int i = 0; i < _videos.Count; i++)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine("\nProcessing cancelled.");
                    break;
                }

                // Clear the current line for the progress bar
                ClearCurrentLine();
                Console.WriteLine($"Processing video {i + 1} of {_videos.Count}: {Path.GetFileName(_videos[i].Input)}");

                try
                {
                    // Prepare the video for processing
                    PrepareVideo(_videos[i]);

                    // Process the video
                    ProcessVideo(_videos[i]);

                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        _successfulVideos++;
                        Console.WriteLine($"Successfully processed: {Path.GetFileName(_videos[i].Input)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing video: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }

                _processedVideos++;
            }

            // Cancel the progress bar update task
            _cancellationTokenSource.Cancel();

            // Wait for the progress task to complete
            try
            {
                progressTask.Wait();
            }
            catch (AggregateException)
            {
                // Task was cancelled, which is expected
            }

            // Clear the progress bar line
            ClearCurrentLine();

            Console.WriteLine("======================================");
            Console.WriteLine($"Processing complete. Successfully processed {_successfulVideos} of {_totalVideos} videos.");

            // Log the output locations
            if (_successfulVideos > 0)
            {
                Console.WriteLine("\nProcessed files can be found at:");
                foreach (var video in _videos.Where(v => File.Exists(v.Output)))
                {
                    Console.WriteLine($"  {video.Output}");
                }
            }
        }

        private async Task UpdateProgressBar()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    // Calculate progress percentage
                    double progress = (double)_processedVideos / _totalVideos;

                    // Draw the progress bar
                    DrawProgressBar(progress);

                    // Wait a bit before updating again
                    await Task.Delay(100, _cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, which is expected
            }
        }

        private void DrawProgressBar(double progress)
        {
            const int progressBarWidth = 50;

            // Calculate the number of filled positions in the progress bar
            int filledWidth = (int)Math.Round(progress * progressBarWidth);

            // Build the progress bar string
            StringBuilder progressBar = new StringBuilder();
            progressBar.Append("[");

            for (int i = 0; i < progressBarWidth; i++)
            {
                if (i < filledWidth)
                    progressBar.Append("=");
                else
                    progressBar.Append(" ");
            }

            progressBar.Append("]");

            // Add percentage and counts
            progressBar.Append($" {progress:P0} ({_processedVideos}/{_totalVideos})");

            // Clear the current line and write the progress bar
            ClearCurrentLine();
            Console.Write(progressBar.ToString());
        }

        private void ClearCurrentLine()
        {
            // Move to the beginning of the line and clear it
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
        }

        private void PrepareVideo(Video video)
        {
            // Remove read-only attributes if present
            Tool.RemoveReadOnlyProperty(video.Input);
            Tool.RemoveReadOnlyProperty(video.SubtitlesPathOriginal);

            // Read subtitle file
            video.SubtitlesOriginal = Subtitles.Read(video.SubtitlesPathOriginal);

            // Get swear words from settings
            video.SwearWordsFull = GetSwearWordsFull();
            video.SwearWordsSingle = GetSwearWordsSingle();

            // Set single word timing settings
            video.SingleWordBefore = Tool.Settings.SingleWordBefore;
            video.SingleWordAfter = Tool.Settings.SingleWordAfter;

            // Get subtitle intervals
            video.Subtitles = Subtitles.GetIntervals(video);

            // Log the number of intervals that will be muted
            int intervals = video.Subtitles.Count(s => s.RemoveFlag);
            Console.WriteLine($"Found {intervals} intervals to mute");
        }

        private List<string> GetSwearWordsFull()
        {
            var words = new List<string>();

            // Add words from settings
            if (Tool.Settings.WordFullAss) words.Add("ass");
            if (Tool.Settings.WordFullAsshole) words.Add("asshole");
            if (Tool.Settings.WordFullBastard) words.Add("bastard");
            if (Tool.Settings.WordFullBitch) words.Add("bitch");
            if (Tool.Settings.WordFullBullshit) words.Add("bullshit");
            if (Tool.Settings.WordFullChrist) words.Add("christ");
            if (Tool.Settings.WordFullCock) words.Add("cock");
            if (Tool.Settings.WordFullCunt) words.Add("cunt");
            if (Tool.Settings.WordFullDamn) words.Add("damn");
            if (Tool.Settings.WordFullDick) words.Add("dick");
            if (Tool.Settings.WordFullDickhead) words.Add("dickhead");
            if (Tool.Settings.WordFullFuck) words.Add("fuck");
            if (Tool.Settings.WordFullGod) words.Add("god");
            if (Tool.Settings.WordFullGoddamn) words.Add("goddamn");
            if (Tool.Settings.WordFullJesus) words.Add("jesus");
            if (Tool.Settings.WordFullMotherfucker) words.Add("motherfucker");
            if (Tool.Settings.WordFullPussy) words.Add("pussy");
            if (Tool.Settings.WordFullShit) words.Add("shit");

            // Add custom words
            if (Tool.Settings.WordFullCustom != null)
            {
                words.AddRange(Tool.Settings.WordFullCustom);
            }

            return words;
        }

        private List<string> GetSwearWordsSingle()
        {
            var words = new List<string>();

            // Add words from settings
            if (Tool.Settings.WordSingleAss) words.Add("ass");
            if (Tool.Settings.WordSingleAsshole) words.Add("asshole");
            if (Tool.Settings.WordSingleBastard) words.Add("bastard");
            if (Tool.Settings.WordSingleBitch) words.Add("bitch");
            if (Tool.Settings.WordSingleBullshit) words.Add("bullshit");
            if (Tool.Settings.WordSingleChrist) words.Add("christ");
            if (Tool.Settings.WordSingleCock) words.Add("cock");
            if (Tool.Settings.WordSingleCunt) words.Add("cunt");
            if (Tool.Settings.WordSingleDamn) words.Add("damn");
            if (Tool.Settings.WordSingleDick) words.Add("dick");
            if (Tool.Settings.WordSingleDickhead) words.Add("dickhead");
            if (Tool.Settings.WordSingleFuck) words.Add("fuck");
            if (Tool.Settings.WordSingleGod) words.Add("god");
            if (Tool.Settings.WordSingleGoddamn) words.Add("goddamn");
            if (Tool.Settings.WordSingleJesus) words.Add("jesus");
            if (Tool.Settings.WordSingleMotherfucker) words.Add("motherfucker");
            if (Tool.Settings.WordSinglePussy) words.Add("pussy");
            if (Tool.Settings.WordSingleShit) words.Add("shit");

            // Add custom words
            if (Tool.Settings.WordSingleCustom != null)
            {
                words.AddRange(Tool.Settings.WordSingleCustom);
            }

            return words;
        }

        private void ProcessVideo(Video video)
        {
            bool createSubtitles = Tool.Settings.CreateSubtitles;
            bool embedSubtitles = Tool.Settings.EmbedSubtitles;
            bool deleteOriginalFiles = Tool.Settings.DeleteOriginalFiles;
            bool muteVocalsOnly = Tool.Settings.MuteVocalsOnly;

            SubtitlesType subtitlesType = SubtitlesType.Normal;
            if (Tool.Settings.ExclusiveSubtitles) subtitlesType = SubtitlesType.Exclusive;
            if (Tool.Settings.BothSubtitles) subtitlesType = SubtitlesType.Both;

            // Create subtitles if requested
            if (createSubtitles && !_cancellationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Creating subtitles...");
                Subtitles.Create(video, embedSubtitles, subtitlesType);
            }

            // Create the video
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Processing video...");
                FFMPEG.Mute(video, embedSubtitles && createSubtitles, subtitlesType, deleteOriginalFiles, muteVocalsOnly);
            }
        }
    }
}
