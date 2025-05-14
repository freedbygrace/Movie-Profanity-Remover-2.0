# Movie Profanity Remover 2.0 - CLI Interface

This command-line interface allows for batch processing of video files to remove profanity by muting audio during specific intervals identified in subtitle files.

## Usage

```
MovieProfanityRemoverCLI [options]
```

## Options

### Required Options

- `-i, --input <directory>`: Input directory containing video files to process.

### Optional Options

#### Directory and File Options
- `-r, --recursive`: Search for video files recursively in subdirectories. (Default: false)
- `-d, --depth <number>`: Maximum recursion depth when searching for video files. Use -1 for unlimited. (Default: -1)
- `-p, --pattern <regex>`: Regex pattern for matching subtitle files to video files. Supports tokens like {Name}, {BaseName}, {Extension}, etc. (Default: "({BaseName}).*\\.(srt)")
- `-o, --output <directory>`: Output directory for processed files. (Default: same as input)
- `-c, --config <file>`: Path to a configuration file with processing settings.
- `--filter <file>`: Path to a text file containing profanity words to filter, one per line.

#### Subtitle Options
- `--create-subtitles`: Create new subtitle files with censored text. (Default: true)
- `--embed-subtitles`: Embed subtitles in the output video file. (Default: true)
- `--subtitles-type <type>`: Type of subtitles to create: normal, exclusive, or both. (Default: "normal")

#### Video Options
- `--output-type <format>`: Output video file format (mp4, mkv, etc.). (Default: "mp4")
- `--delete-originals`: Delete original video and subtitle files after processing. (Default: false)
- `--mute-vocals-only`: Attempt to mute only vocal channels, preserving background audio. (Default: false)
- `--custom-affix <string>`: Custom affix to add to output filenames. (Default: "_SL")
- `--aspect-ratio`: Apply custom aspect ratio to output video. (Default: false)
- `--aspect-width <number>`: Aspect ratio width. (Default: 16)
- `--aspect-height <number>`: Aspect ratio height. (Default: 9)

#### Word Filtering Options
- `--single-word`: Enable single word filtering. (Default: false)
- `--single-word-before <ms>`: Milliseconds to mute before a single profanity word. (Default: 500)
- `--single-word-after <ms>`: Milliseconds to mute after a single profanity word. (Default: 500)

#### Utility Options
- `--list-tokens`: List all available tokens for subtitle pattern matching.
- `--verbose`: Enable verbose output. (Default: false)
- `--dry-run`: Perform a dry run without actually processing videos. (Default: false)
- `--log-file <file>`: Path to a log file to write output.

## Examples

### Basic Usage

Process all video files in a directory:

```
MovieProfanityRemoverCLI -i "C:\Videos"
```

### Recursive Processing

Process all video files in a directory and its subdirectories:

```
MovieProfanityRemoverCLI -i "C:\Videos" -r
```

### Custom Subtitle Matching

Use a custom regex pattern to match subtitle files:

```
MovieProfanityRemoverCLI -i "C:\Videos" -p "{BaseName}.*\.srt"
```

### Custom Filter Words

Use a custom list of profanity words to filter:

```
MovieProfanityRemoverCLI -i "C:\Videos" --filter "C:\filter_words.txt"
```

### Output to Different Directory

Process videos and save the output to a different directory:

```
MovieProfanityRemoverCLI -i "C:\Videos" -o "C:\Processed"
```

### Logging to File

Save the processing log to a file:

```
MovieProfanityRemoverCLI -i "C:\Videos" --log-file "C:\logs\processing.log"
```

### Dry Run

Perform a dry run to see what would be processed without actually processing any videos:

```
MovieProfanityRemoverCLI -i "C:\Videos" -r --dry-run
```

### List Available Tokens

List all available tokens for subtitle pattern matching:

```
MovieProfanityRemoverCLI --list-tokens
```

### Complete Example

Process all video files recursively, with custom subtitle matching, output directory, and filter words:

```
MovieProfanityRemoverCLI -i "C:\Videos" -r -d 2 -p "{BaseName}.*\.srt" -o "C:\Processed" --filter "C:\filter_words.txt" --subtitles-type both --output-type mkv --delete-originals --custom-affix "_Clean" --verbose
```

## Subtitle Matching with Tokens

By default, the CLI will look for subtitle files with the same base name as the video file and a `.srt` extension. You can customize this behavior using the `-p, --pattern` option.

The pattern is a regular expression that will be applied to the filenames in the same directory as the video file. You can use various tokens in the pattern, which will be replaced with values from the video file.

### Available Tokens

- `{Name}` - The filename with extension (e.g., 'movie.mp4')
- `{FileName}` - Alias for Name
- `{BaseName}` - The filename without extension (e.g., 'movie')
- `{Extension}` - The file extension (e.g., '.mp4')
- `{FullName}` - The full path to the file
- `{DirectoryName}` - The name of the directory containing the file
- `{Length}` - The file size in bytes
- `{CreationTime}` - The file creation time (format: yyyy-MM-dd_HH-mm-ss)
- `{LastWriteTime}` - The file last write time (format: yyyy-MM-dd_HH-mm-ss)
- `{LastAccessTime}` - The file last access time (format: yyyy-MM-dd_HH-mm-ss)
- `{DirectoryName}` - The name of the directory containing the file
- `{DirectoryFullName}` - The full path to the directory containing the file
- `{ParentDirectory}` - The name of the parent directory
- `{ParentDirectoryFullName}` - The full path to the parent directory
- `{Drive}` - The drive letter or root path
- `{PathWithoutExtension}` - The full path without the file extension
- `{RelativePath}` - The path relative to the current directory

### Example Patterns

- `{BaseName}.*\.srt` - Match any .srt file with the same base name as the video
- `{BaseName}\.eng\.srt` - Match English subtitle files (movie.eng.srt)
- `{BaseName}(_.*)?\\.(srt|sub|idx)` - Match multiple subtitle formats with optional suffix

All tokens are case-insensitive, and the values are automatically escaped for use in regular expressions.

## Regex Filter Patterns

The CLI interface uses JSON configuration files for regex filter patterns. This provides a clean, structured way to define patterns for profanity filtering.

### JSON Configuration File

You specify a JSON configuration file using the `--regex-config` option:

```json
{
  "Name": "My Profanity Filter",
  "Description": "Custom configuration with profanity patterns",
  "IncludePatterns": [
    "(^.*ass.*$)",
    "(^.*fuck.*$)",
    "(^.*shit.*$)",
    "(^.*bitch.*$)",
    "(^.*damn.*$)"
  ],
  "CaseSensitive": false,
  "AssumeWordBoundary": false
}
```

### Configuration Properties

- **Name**: A name for your configuration
- **Description**: A description of your configuration
- **IncludePatterns**: An array of regex patterns to match
- **CaseSensitive**: Whether to perform case-sensitive matching (default: false)
- **AssumeWordBoundary**: Whether to automatically add word boundaries to patterns (default: false)

### Pattern Format

The recommended pattern format is `(^.*WORD.*$)`, where `WORD` is the profanity word you want to match. This format matches any line that contains the word, regardless of where it appears in the line.

For example:
- `(^.*ass.*$)` matches any line containing "ass"
- `(^.*fuck.*$)` matches any line containing "fuck"

This simple format is easy to understand and maintain, while still being effective for profanity filtering.

### Word Boundaries

The `AssumeWordBoundary` property is available but set to `false` by default. When set to `true`, the application automatically adds word boundaries (`\b`) to the beginning and end of each pattern that doesn't already have them.

However, with the recommended `(^.*WORD.*$)` format, word boundaries are not necessary and should not be used.

The JSON format provides several advantages:
- Clean, structured format for defining patterns
- Ability to name and describe your configuration
- Easier to share and reuse configurations
- Support for advanced features like word boundary handling

### Command Line Options for Regex Filtering

- `--use-regex`: Enable regex pattern matching for profanity filtering (requires a filter file)
- `--regex-config <file>`: Path to a JSON file containing regex filter configuration

### Creating and Saving Configurations

The CLI provides options to create and save filter configurations:

- `--create-default-config <file>`: Create a default regex filter configuration and save it to a JSON file
- `--save-regex-config <file>`: Save the current regex filter configuration (from command line or loaded file) to a JSON file

This allows you to:
1. Create a starting configuration with `--create-default-config`
2. Modify it to suit your needs
3. Use it in future runs with `--regex-config`

### Examples

```
# Use a JSON configuration file
MovieProfanityRemoverCLI -i "C:\Videos" --regex-config "C:\my_filter_config.json"

# Create a default configuration file with basic profanity patterns
MovieProfanityRemoverCLI --create-default-config "C:\default_filter_config.json"

# Process videos with regex filtering enabled (uses default patterns)
MovieProfanityRemoverCLI -i "C:\Videos" --use-regex

# Process videos with a JSON configuration and save any modifications
MovieProfanityRemoverCLI -i "C:\Videos" --regex-config "C:\my_filter_config.json" --save-regex-config "C:\updated_config.json"
```

If no filter file is specified, the application will use the default filter settings from the application configuration.

## Subtitles Types

The application supports three types of subtitles:

- `normal` - Regular subtitles with profanity words replaced by asterisks
- `exclusive` - Only subtitles containing profanity words are included
- `both` - Both normal and exclusive subtitles are created

## Notes

- The CLI interface uses the same core processing logic as the GUI application.
- Settings are saved between runs, so you only need to specify options that you want to change from the previous run.
- The application requires FFMPEG, which is included with the distribution.
- Processing large video files can be time-consuming and CPU-intensive.
- The progress bar shows the overall progress of the batch processing.
- Use the `--verbose` option to see more detailed information during processing.
- Use the `--dry-run` option to test your settings without actually processing any videos.
- Use the `--log-file` option to save the processing log to a file.
