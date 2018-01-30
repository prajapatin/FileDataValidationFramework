using System;
using System.IO;

namespace ValidationFramework.Core.CsvHelper
{
    /// <summary>
    /// CSV file destination class which accepts steam, file path, fullname etc.
    /// </summary>
    public class CSVDestination
    {
        public StreamWriter StreamWriter;

        /// <summary>
        /// Provides uage of direct file path wherever this class is used as a type
        /// </summary>
        /// <param name="path">Implicit value of file path</param>
        public static implicit operator CSVDestination(string path)
        {
            return new CSVDestination(path);
        }

        /// <summary>
        /// Contsructor accepting stream writer as a paramter
        /// </summary>
        /// <param name="streamWriter">File stream writer</param>
        private CSVDestination(StreamWriter streamWriter)
        {
            this.StreamWriter = streamWriter;
        }

        /// <summary>
        /// Contsructor accepting stream as a paramter
        /// </summary>
        /// <param name="stream">File stream</param>
        private CSVDestination(Stream stream)
        {
            this.StreamWriter = new StreamWriter(stream);
        }

        /// <summary>
        /// Contsructor accepting stream full file name with path as a parameter
        /// </summary>
        /// <param name="fullName">File path with name</param>
        public CSVDestination(string fullName)
        {
            FixCsvFileName(ref fullName);
            this.StreamWriter = new StreamWriter(fullName);
        }

        private static void FixCsvFileName(ref string fullName)
        {
            fullName = Path.GetFullPath(fullName);
            var path = Path.GetDirectoryName(fullName);
            if (path != null && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!String.Equals(Path.GetExtension(fullName), ".csv"))
            {
                fullName += ".csv";
            }
        }
    }
}
