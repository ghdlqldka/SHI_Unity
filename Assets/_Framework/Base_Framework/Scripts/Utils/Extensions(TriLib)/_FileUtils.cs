using System;
using System.IO;
using UnityEngine;

// namespace TriLib
namespace _Base_Framework
{
    /// <summary>
    /// Contains file helper functions.
    /// </summary>
	public static class _FileUtils
    {
        // private static string LOG_FORMAT = "<color=magenta><b>[_FileUtils]</b></color> {0}";

        /// <summary>
        /// Gets the short filename from a full filename.
        /// </summary>
        /// <returns>The short filename.</returns>
        /// <param name="filename">Full filename.</param>
        public static string GetShortFilename(string filename)
        {
            return FileUtils.GetShortFilename(filename);
        }

        /// <summary>
        /// Gets the file directory.
        /// </summary>
        /// <returns>The file directory.</returns>
        /// <param name="filename">Full filename.</param>
        public static string GetFileDirectory(string filename)
        {
            return FileUtils.GetFileDirectory(filename);
        }

        /// <summary>
        /// Gets the filename without extension.
        /// </summary>
        /// <returns>The filename without extension.</returns>
        /// <param name="filename">Full filename.</param>
        public static string GetFilenameWithoutExtension(string filename)
        {
            return FileUtils.GetFilenameWithoutExtension(filename);
        }

        /// <summary>
        /// Gets the filename extension.
        /// </summary>
        /// <returns>The filename extension.</returns>
        /// <param name="filename">Full filename.</param>
        public static string GetFileExtension(string filename)
        {
            return FileUtils.GetFileExtension(filename);
        }

        /// <summary>
        /// Gets the path filename.
        /// </summary>
        /// <returns>The filename.</returns>
        /// <param name="path">Path.</param>
        public static string GetFilename(string path)
        {
            return FileUtils.GetFilename(path);
        }

        /// <summary>
        /// Synchronously loads the file data.
        /// </summary>
        /// <returns>The file data.</returns>
        /// <param name="filename">Filename.</param>
        public static byte[] LoadFileData(string filename)
        {
            return FileUtils.LoadFileData(filename);
        }

        /// <summary>
        /// Creates a file stream for given filename.
        /// </summary>
        /// <returns>The created FileStream.</returns>
        /// <param name="filename">Filename.</param>
        public static FileStream LoadFileStream(string filename)
        {
            return FileUtils.LoadFileStream(filename);
        }
    }
}

