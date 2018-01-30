using System.Collections.Generic;
using System.Threading.Tasks;

namespace ValidationFramework.Core.CsvHelper
{
    /// <summary>
    /// CSVFile extension methods
    /// </summary>
    public static class CSVFileLinqExtensions
    {
        /// <summary>
        /// Converts IEnumerable of object of type T into CSV record
        /// </summary>
        /// <typeparam name="T">Type to be used to covert object into CSV record</typeparam>
        /// <param name="source">Scource list of object of type T</param>
        /// <param name="csvDestination">Destination where CSV file would be written</param>
        public static async Task ToCsv<T>(this IEnumerable<T> source, CSVDestination csvDestination) where T : BaseEntity
        {
            await source.ToCsv(csvDestination, null);
        }

        /// <summary>
        /// Converts IEnumerable of object of type T into CSV record
        /// </summary>
        /// <typeparam name="T">Type to be used to covert object into CSV record</typeparam>
        /// <param name="source">Scource list of object of type T</param>
        /// <param name="csvDestination">Destination where CSV file would be written</param>
        /// <param name="csvConfiguration">Configuration to be used while writing CSV file</param>
        public static async Task ToCsv<T>(this IEnumerable<T> source, CSVDestination csvDestination, CSVConfiguration csvConfiguration) where T : BaseEntity
        {
            using (var csvFile = new CSVFile<T>(csvDestination, csvConfiguration))
            {
                foreach (var record in source)
                {
                   await csvFile.Append(record);
                }
            }
        }

    }
}
