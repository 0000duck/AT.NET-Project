using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWatcher
{
    // Custom class for file work
    public static class FileHelper
    {      
        
        /// <summary>
        /// Checks if a file is still being used (hasn't been completely transfered to the folder)
        /// </summary>
        /// <param name="path">The file to check</param>
        /// <returns><c>True</c> if the file is being used, <c>False</c> if now</returns>
        public static bool FileIsUsed(string path)
        {
            FileStream stream = null;
            string name = null;

            try
            {
                var fi = new FileInfo(path);
                name = fi.Name;
                stream = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                if (name != null)
                    return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            
            return false;
        }


        /// <summary>
        /// Checks a filename for chars that wont work with most servers
        /// <param name="name">Name of file</param>
        /// </summary>
        public static bool IsAllowedFilename(string name)
        {
            return name.ToCharArray().All(IsAllowedChar) && IsNotReservedName(name);
        }

        /// <summary>
        /// Checks if a char is allowed, based on the allowed chars for filenames
        /// <param name="ch">Char to check</param>
        /// </summary>
        private static bool IsAllowedChar(char ch)
        {
            return !Path.GetInvalidFileNameChars().Any(ch.Equals);
        }

        /// <summary>
        /// Checks if the given file name is one of the system-reserved names, found on internet
        /// <param name="name">Name to check</param>
        /// </summary>
        private static bool IsNotReservedName(string name)
        {
            return !new[]
                {
                    "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                    "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                    "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
                }.Any(name.Equals);
        }
    }
}
