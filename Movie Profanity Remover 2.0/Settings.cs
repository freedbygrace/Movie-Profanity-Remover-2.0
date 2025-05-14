using System;
using System.Collections.Generic;

namespace Movie_Profanity_Remover_2._0
{
    [Serializable]
    public class Settings
    {
        public bool AspectRatio = false;
        public int AspectRatioWidth = 16;
        public int AspectRatioHeight = 9;

        public string OutputType = "mp4";
        public string CustomAffix = "_SL";

        public bool SingleWord = false;
        public int SingleWordBefore = 500;
        public int SingleWordAfter = 500;

        public bool CreateSubtitles = true;
        public bool EmbedSubtitles = true;
        public bool DeleteOriginalFiles = false;
        public bool MuteVocalsOnly = false;

        public bool NormalSubtitles = true;
        public bool ExclusiveSubtitles = false;
        public bool BothSubtitles = false;

        public bool WordFullAss = false;
        public bool WordFullAsshole = false;
        public bool WordFullBastard = false;
        public bool WordFullBitch = false;
        public bool WordFullBullshit = false;
        public bool WordFullChrist = false;
        public bool WordFullCock = false;
        public bool WordFullCunt = false;
        public bool WordFullDamn = false;
        public bool WordFullDick = false;
        public bool WordFullDickhead = false;
        public bool WordFullFuck = false;
        public bool WordFullGod = false;
        public bool WordFullGoddamn = false;
        public bool WordFullJesus = false;
        public bool WordFullMotherfucker = false;
        public bool WordFullPussy = false;
        public bool WordFullShit = false;

        public List<string> WordFullCustom = new List<string>();

        // Regex patterns for filtering
        public List<string> RegexIncludePatterns = new List<string>();

        // Flag to enable regex filtering
        public bool UseRegexFiltering = false;

        public bool WordSingleAss = false;
        public bool WordSingleAsshole = false;
        public bool WordSingleBastard = false;
        public bool WordSingleBitch = false;
        public bool WordSingleBullshit = false;
        public bool WordSingleChrist = false;
        public bool WordSingleCock = false;
        public bool WordSingleCunt = false;
        public bool WordSingleDamn = false;
        public bool WordSingleDick = false;
        public bool WordSingleDickhead = false;
        public bool WordSingleFuck = false;
        public bool WordSingleGod = false;
        public bool WordSingleGoddamn = false;
        public bool WordSingleJesus = false;
        public bool WordSingleMotherfucker = false;
        public bool WordSinglePussy = false;
        public bool WordSingleShit = false;

        public List<string> WordSingleCustom = new List<string>();

        // Regex patterns for single word filtering
        public List<string> SingleRegexIncludePatterns = new List<string>();
        public List<string> SingleRegexExcludePatterns = new List<string>();
    }
}
