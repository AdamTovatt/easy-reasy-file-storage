namespace EasyReasy.FileStorage
{
    /// <summary>
    /// Defines the mode for opening a file for writing operations.
    /// </summary>
    public enum FileWriteMode
    {
        /// <summary>
        /// Overwrites the file completely if it exists, or creates a new file if it doesn't exist.
        /// This is the default behavior for replacing file content.
        /// </summary>
        Overwrite,

        /// <summary>
        /// Opens the file for appending. All writes will be performed at the end of the file.
        /// Seeking to arbitrary positions is not supported in this mode.
        /// </summary>
        Append,

        /// <summary>
        /// Opens the file for random access writing. Preserves existing content and allows
        /// seeking to arbitrary positions within the file. Essential for chunked uploads
        /// and other scenarios requiring random-access writing.
        /// </summary>
        RandomAccess
    }
}
