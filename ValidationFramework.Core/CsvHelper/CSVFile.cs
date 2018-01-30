using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ValidationFramework.Core.CsvHelper
{
    /// <summary>
    /// CSV file class with file reading/writing capabilities
    /// </summary>
    public class CSVFile : IDisposable
    {
               
        #region Static Members

        /// <summary>
        /// Property to provide default CSV configuration
        /// </summary>
        public static CSVConfiguration DefaultCsvConfiguration { get; set; }
        public static bool FastIndexOfAny { get; set; }

        /// <summary>
        /// Parameter less constructor
        /// </summary>
        static CSVFile()
        {
            DefaultCsvConfiguration = new CSVConfiguration();
            FastIndexOfAny = true;
        }

        #endregion

        /// <summary>
        /// CSV file field separator (e.g. , or ;)
        /// </summary>
        public char FieldSeparator { get; private set; }

        /// <summary>
        /// Columns extracted from CSV file
        /// </summary>
        public IEnumerable<String> Columns { get; private set; }

        /// <summary>
        /// Non-default Text qualifier to be used
        /// </summary>
        public char TextQualifier { get; private set; }

        internal protected Stream BaseStream;
        protected static DateTime DateTimeZero = new DateTime();
        
        /// <summary>
        /// Redas file - object by object
        /// </summary>
        /// <typeparam name="T">Type for which CSV file is to be parsed</typeparam>
        /// <param name="csvFilePath">Full file path</param>
        /// <returns>IEnumerable list of Object of type T</returns>
        public static IEnumerable<T> Read<T>(string csvFilePath) where T : BaseEntity, new()
        {
            var csvFileReader = new CSVFileReader<T>(csvFilePath);
            return (IEnumerable<T>)csvFileReader;
        }
          

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // overriden in derived classes
        }
    }

    /// <summary>
    /// Generic implementation for CSVFile class
    /// </summary>
    /// <typeparam name="T">Type for which CSVFile object to be initialized</typeparam>
    public class CSVFile<T> : CSVFile where T : BaseEntity
    {
        private readonly char fieldSeparator;
        private readonly string fieldSeparatorAsString;
        private readonly char[] invalidCharsInFields;
        private readonly StreamWriter streamWriter;
        private readonly char textQualifier;
        private readonly String[] columns;
        private Func<T, object>[] getters;
        readonly bool[] isInvalidCharInFields;
        
        /// <summary>
        /// Constructor taking CSVDestination as a parameter
        /// </summary>
        /// <param name="csvDestination">Instance of CSVDestination</param>
        public CSVFile(CSVDestination csvDestination)
            : this(csvDestination, null)
        {
        }

        /// <summary>
        /// Default parameter less constructor
        /// </summary>
        public CSVFile()
        {
        }

        /// <summary>
        /// Constructor with cv destination object and csv configuration object as a parameter
        /// </summary>
        /// <param name="csvDestination">Destination where file is to be written</param>
        /// <param name="csvConfiguration">Configuration parameters to be used reading/writing CSV files</param>
        public CSVFile(CSVDestination csvDestination, CSVConfiguration csvConfiguration)
        {
            if (csvConfiguration == null)
            {
                csvConfiguration = DefaultCsvConfiguration;
            }
            this.columns = (csvConfiguration.Columns ?? InferColumns(typeof(T))).ToArray();
            this.fieldSeparator = csvConfiguration.FieldSeparator;
            this.fieldSeparatorAsString = this.fieldSeparator.ToString(CultureInfo.InvariantCulture);
            this.textQualifier = csvConfiguration.TextQualifier;
            this.streamWriter = csvDestination.StreamWriter;

            this.invalidCharsInFields = new[] { '\r', '\n', this.textQualifier, this.fieldSeparator };
            this.isInvalidCharInFields = new bool[256];

            foreach (var c in this.invalidCharsInFields)
            {
                this.isInvalidCharInFields[c] = true;
            }
            this.WriteHeader();

            this.CreateGetters();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
               this.streamWriter.Close();
            }
        }

        protected static IEnumerable<string> InferColumns(Type recordType)
        {
            var columns = recordType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi => pi.GetIndexParameters().Length == 0
                    && pi.GetSetMethod() != null
                    && !Attribute.IsDefined(pi, typeof(CsvIgnoreAttribute)))
                .Select(pi => pi.Name)
                .Concat(recordType
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(fi => !Attribute.IsDefined(fi, typeof(CsvIgnoreAttribute)))
                    .Select(fi => fi.Name))
                .ToList();
            return columns;
        }

        /// <summary>
        /// Appends record of Type T in file destination after converting it to CSV format
        /// </summary>
        /// <param name="record">Instance of type T</param>
        public async Task Append(T record)
        {
            var csvLine = this.ToCsv(record);
            await this.streamWriter.WriteLineAsync(csvLine);
        }

        private static Func<T, object> FindGetter(string c, bool staticMember)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | (staticMember ? BindingFlags.Static : BindingFlags.Instance);
            Func<T, object> func = null;
            PropertyInfo pi = typeof(T).GetProperty(c, flags);
            FieldInfo fi = typeof(T).GetField(c, flags);

            if (pi != null)
            {
                func = o => pi.GetValue(o, null);
            }
            else if (fi != null)
            {
                func = o => fi.GetValue(o);
            }
            return func;
        }


        private List<Func<T, object>> FindErrorEntityGetters(string[] columns)
        {
            List<Func<T, object>> errorEntityGetters = new List<Func<T, object>>();
            foreach(string column in columns)
            {
                Func<T, object> func = o =>
                {
                    var errObj = o as ErrorEntity;
                    if(column == "Description")
                    {
                        return errObj.Description;
                    }
                    var propertyName = (column.IndexOf(' ') < 0 ? column : column.Replace(" ", ""));
                    var entityProperty = errObj.EntityProperties[propertyName];
                    if (entityProperty != null)
                    {
                        return entityProperty.PropertyValue;
                    }
                    return null;
                };
                errorEntityGetters.Add(func);
            }

            return errorEntityGetters;
        }
        private void CreateGetters()
        {
            var list = new List<Func<T, object>>();
            if (typeof(T) != typeof(ErrorEntity))
            {
                foreach (var columnName in columns)
                {
                    Func<T, Object> func = null;
                    var propertyName = (columnName.IndexOf(' ') < 0 ? columnName : columnName.Replace(" ", ""));
                    func = FindGetter(propertyName, false) ?? FindGetter(propertyName, true);
                    if (func != null)
                    {
                        list.Add(func);
                    }
                }
            }
            else if (typeof(T) == typeof(ErrorEntity))
            {
                var errorEntityGetters = FindErrorEntityGetters(columns);
                if (errorEntityGetters != null)
                {
                    list.AddRange(errorEntityGetters);
                }

            }
            this.getters = list.ToArray();
        }

        private string ToCsv(T record)
        {
            if (record == null)
            {
                throw new ArgumentException("Cannot be null", "record");
            }

            string[] csvStrings = new string[getters.Length];

            for (int i = 0; i < getters.Length; i++)
            {
                var getter = getters[i];
                object fieldValue = getter == null ? null : getter(record);
                csvStrings[i] = this.ToCsvString(fieldValue);
            }
            return string.Join(this.fieldSeparatorAsString, csvStrings);

        }

        private string ToCsvString(object o)
        {
            if (o != null)
            {
                string valueString = o as string ?? Convert.ToString(o, CultureInfo.CurrentUICulture);
                if (RequiresQuotes(valueString))
                {
                    var csvLine = new StringBuilder();
                    csvLine.Append(this.textQualifier);
                    foreach (char c in valueString)
                    {
                        if (c == this.textQualifier)
                        {
                            csvLine.Append(c); // double the double quotes
                        }
                        csvLine.Append(c);
                    }
                    csvLine.Append(this.textQualifier);
                    return csvLine.ToString();
                }
                else
                {
                    return valueString;
                }
            }
            return string.Empty;
        }

        private bool RequiresQuotes(string valueString)
        {
            if (CSVFile.FastIndexOfAny)
            {
                var len = valueString.Length;
                for (int i = 0; i < len; i++)
                {
                    char c = valueString[i];
                    if (c <= 255 && this.isInvalidCharInFields[c])
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return valueString.IndexOfAny(this.invalidCharsInFields) >= 0;
            }
        }

        private void WriteHeader()
        {
            var csvLine = new StringBuilder();
            for (int i = 0; i < this.columns.Length; i++)
            {
                if (i > 0)
                {
                    csvLine.Append(this.fieldSeparator);
                }
                csvLine.Append(this.ToCsvString(this.columns[i]));
            }
            this.streamWriter.WriteLine(csvLine.ToString());
        }
    }

    public class CsvIgnoreAttribute : Attribute
    {
        public override string ToString()
        {
            return "Ignore Property";
        }
    }
}
