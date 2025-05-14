namespace Movie_Profanity_Remover_2._0
{
    /// <summary>
    /// Defines the type of subtitles to create.
    /// </summary>
    public enum SubtitlesType
    {
        /// <summary>
        /// Normal subtitles with profanity censored.
        /// </summary>
        Normal,
        
        /// <summary>
        /// Exclusive subtitles that only show profanity.
        /// </summary>
        Exclusive,
        
        /// <summary>
        /// Both normal and exclusive subtitles.
        /// </summary>
        Both
    }
}
