using ValidationFramework.Core.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ValidationFramework.Core.CsvHelper
{
    /// <summary>
    /// Generic CSV file reader
    /// </summary>
    /// <typeparam name="T">Type of any object which inherits from BaseEntity</typeparam>
    public class CSVFileReader<T> : CSVFile, IEnumerable<T>, IEnumerator<T> where T : BaseEntity, new()
    {
        private readonly Dictionary<Type, List<Action<T, String>>> allSetters = new Dictionary<Type, List<Action<T, String>>>();
        private char curChar;
        private int len;
        private string line;
        private int pos;
        private T record;
        private readonly char fieldSeparator;
        private readonly TextReader textReader;
        private readonly char textQualifier;
        private readonly StringBuilder parseFieldResult = new StringBuilder();

        private string[] _normalizedColumns;
        private List<string> _csvColumns = new List<string>();
        private List<ErrorEntity> _invalidEntities = new List<ErrorEntity>();

        /// <summary>
        /// Provides column names without white space
        /// </summary>
        public string[] NormalizedColumns
        {
            get
            {
                return _normalizedColumns;
            }

            set
            {
                _normalizedColumns = value;
            }
        }

        /// <summary>
        /// Columns as read from file or supplided from outside
        /// </summary>
        public List<string> CSVColumns
        {
            get
            {
                return _csvColumns;
            }

            set
            {
                _csvColumns = value;
            }
        }

        /// <summary>
        /// List of error entities which is of concrete type regardless of whatever generic type T
        /// </summary>
        public List<ErrorEntity> InvalidEntities
        {
            get
            {
                return _invalidEntities;
            }
        }

        /// <summary>
        /// Constructor accepting csv file path for which reader is to be intitialized
        /// </summary>
        /// <param name="csvFilePath">Fule file path</param>
        public CSVFileReader(string csvFilePath)
            : this(csvFilePath, null)
        {
        }

        /// <summary>
        /// Constructor accepting cv file path and configuration for file read
        /// </summary>
        /// <param name="csvFilePath">Full CSV file path</param>
        /// <param name="csvConfiguration">Configuration to be used for reading CSV file</param>
        public CSVFileReader(string csvFilePath, CSVConfiguration csvConfiguration)
        {
            var streamReader = new StreamReader(csvFilePath);
            if (streamReader != null)
            {
                this.BaseStream = streamReader.BaseStream;
            }
            if (csvConfiguration == null)
            {
                csvConfiguration = DefaultCsvConfiguration;
            }
            this.fieldSeparator = csvConfiguration.FieldSeparator;
            this.textQualifier = csvConfiguration.TextQualifier;

            this.textReader = new StreamReader(this.BaseStream);// new FileStream(csvSource.TextReader, FileMode.Open);

            this.ReadHeader(csvConfiguration.Header);

            this.NormalizeColumns();

        }

        /// <summary>
        /// Provides current object of type T for which CSV is read
        /// </summary>
        public T Current
        {
            get { return this.record; }
        }

        /// <summary>
        /// Indicates whether it is an end of file
        /// </summary>
        public bool Eof
        {
            get { return this.line == null; }
        }

        /// <summary>
        /// Provides enumerator to enumerat current file object
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        /// <summary>
        /// Iterates record from file by one
        /// </summary>
        /// <returns>Whether the iterated object is available or not</returns>
        public bool MoveNext()
        {
            return IterateItem().ContinueWith((t) =>
            {
                return (this.record != null);
            }).Result;
            
        }

        public bool HasRecords()
        {
            this.ReadNextLine();
            if (this.line == null && (this.line = this.textReader.ReadLine()) == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Not implemented method
        /// </summary>
        public void Reset()
        {
            throw new NotImplementedException("Cannot reset CSVFileReader enumeration.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                this.textReader.Dispose();
            }
        }

        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private async Task IterateItem()
        {
            bool isErroredOut = false;
            await this.ReadNextLineAsync().ContinueWith(async (t) =>
            {
                if (this.line == null && (this.line = this.textReader.ReadLine()) == null)
                {
                    this.record = default(T);
                }
                else
                {
                    this.record = new T();
                    this.record.RowText = this.line;
                    Type recordType = typeof(T);
                    List<Action<T, String>> setters;
                    if (!this.allSetters.TryGetValue(recordType, out setters))
                    {
                        setters = this.CreateSetters();
                        this.allSetters[recordType] = setters;
                    }

                    var fieldValues = new string[setters.Count];
                    for (int i = 0; i < setters.Count; i++)
                    {
                        fieldValues[i] = this.ParseField();
                        if (this.curChar == this.fieldSeparator)
                        {
                            this.NextChar();
                        }
                        else
                        {
                            break;
                        }
                    }
                    for (int i = 0; i < setters.Count; i++)
                    {
                        try
                        {
                            setters[i]?.Invoke(this.record, fieldValues[i]);
                        }
                        catch
                        {
                            isErroredOut = true;
                            ErrorEntity errorEntiy = new ErrorEntity();
                            errorEntiy.EntityProperties = GetEntityProperties(fieldValues);
                            this._invalidEntities.Add(errorEntiy);

                            break;
                        }
                    }
                    if (isErroredOut)
                    {
                       await IterateItem();
                    }
                }
            }).Result;
            
        }

        private IEntityPropertyCollection GetEntityProperties(string[] fieldValues)
        {
            IEntityPropertyCollection properties = new EntityPropertyCollection();
            for(int i = 0; i< fieldValues.Length; i++)
            {
                properties.Add(new EntityProperty { PropertyName = this._normalizedColumns[i],  PropertyHeader = this._csvColumns[i], PropertyValue = fieldValues[i] });
            }
            return properties;
        }

        private static Action<T, string> EmitSetValueAction(MemberInfo mi, Func<string, object> func)
        {
            var pi = mi as PropertyInfo;
            if (pi != null)
            {
                return (o, v) => pi.SetValue(o, (object)func(v), null);

            }
            
            var fi = mi as FieldInfo;
            if (fi != null)
            {
                return ((o, v) => fi.SetValue(o, func(v)));
            }

            throw new NotImplementedException();
        }

        private static Action<T, string> FindSetter(string c, bool staticMember)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | (staticMember ? BindingFlags.Static : BindingFlags.Instance);
            Action<T, string> action = null;
            PropertyInfo pi = typeof(T).GetProperty(c, flags);
            if (pi != null)
            {
                var pFunc = StringToObject(pi.PropertyType);
                action = EmitSetValueAction(pi, pFunc);
            }
            FieldInfo fi = typeof(T).GetField(c, flags);
            if (fi != null)
            {
                var fFunc = StringToObject(fi.FieldType);
                action = EmitSetValueAction(fi, fFunc);
            }
            return action;
        }

        private static Func<string, object> StringToObject(Type propertyType)
        {
            if (propertyType == typeof(string))
            {
                return (s) =>
                {
                    if (String.IsNullOrEmpty(s))
                    {
                        throw new FormatException();
                    }
                    if(!Regex.IsMatch(s, @"^[a-zA-Z]+$"))
                    {
                        throw new FormatException();
                    }
                    return s;
                };
            }
            else if (propertyType == typeof(Int32))
            {
                return (s) =>  Int32.Parse(s);
            }
            if (propertyType == typeof(DateTime))
            {
                return (s) => DateTime.Parse(s);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private List<Action<T, string>> CreateSetters()
        {
            var list = new List<Action<T, string>>();
            for (int i = 0; i < this._csvColumns.Count; i++)
            {
                string columnName = this._csvColumns[i];
                Action<T, string> action = null;
                if (columnName.IndexOf(' ') >= 0)
                {
                    columnName = columnName.Replace(" ", "");
                }
                action = FindSetter(columnName, false) ?? FindSetter(columnName, true);

                list.Add(action);
            }
            return list;
        }

        private void NextChar()
        {
            if (this.pos < this.len)
            {
                this.pos++;
                this.curChar = this.pos < this.len ? this.line[this.pos] : '\0';
            }
        }

        private void ParseEndOfLine()
        {
            throw new NotImplementedException();
        }

        private string ParseField()
        {
            parseFieldResult.Length = 0;
            if (this.line == null || this.pos >= this.len)
            {
                return null;
            }
            while (this.curChar == ' ' || this.curChar == '\t')
            {
                this.NextChar();
            }
            if (this.curChar == this.textQualifier)
            {
                this.NextChar();
                while (this.curChar != 0)
                {
                    if (this.curChar == this.textQualifier)
                    {
                        this.NextChar();
                        if (this.curChar == this.textQualifier)
                        {
                            this.NextChar();
                            parseFieldResult.Append(this.textQualifier);
                        }
                        else
                        {
                            return parseFieldResult.ToString();
                        }
                    }
                    else if (this.curChar == '\0')
                    {
                        if (this.line == null)
                        {
                            return parseFieldResult.ToString();
                        }
                        this.ReadNextLine();
                    }
                    else
                    {
                        parseFieldResult.Append(this.curChar);
                        this.NextChar();
                    }
                }
            }
            else
            {
                while (this.curChar != 0 && this.curChar != this.fieldSeparator && this.curChar != '\r' && this.curChar != '\n')
                {
                    parseFieldResult.Append(this.curChar);
                    this.NextChar();
                }
            }
            return parseFieldResult.ToString();
        }

        private void ReadHeader(string header)
        {
            if (header == null)
            {
                this.ReadNextLine();
            }
            else
            {
                // we read the first line from the given header
                this.line = header;
                this.pos = -1;
                this.len = this.line.Length;
                this.NextChar();
            }

            string columnName;
            while ((columnName = this.ParseField()) != null)
            {
                this._csvColumns.Add(columnName);
                if (this.curChar == this.fieldSeparator)
                {
                    this.NextChar();
                }
                else
                {
                    break;
                }
            }
        }

        private void NormalizeColumns()
        {
            List<String> filteredColumns = new List<string>();
            for (int i = 0; i < this._csvColumns.Count; i++)
            {
                string columnName = this._csvColumns[i];
                if (columnName.IndexOf(' ') >= 0)
                {
                    columnName = columnName.Replace(" ", "");
                }
                filteredColumns.Add(columnName);
            }
            this._normalizedColumns = filteredColumns.ToArray();
        }

        private async Task ReadNextLineAsync()
        {
            this.line = await this.textReader.ReadLineAsync().ConfigureAwait(false);
            this.pos = -1;
            if (this.line == null)
            {
                this.len = 0;
                this.curChar = '\0';
            }
            else
            {
                this.len = this.line.Length;
                this.NextChar();
            }
        }

        private void ReadNextLine()
        {
            this.line = this.textReader.ReadLine();
            this.pos = -1;
            if (this.line == null)
            {
                this.len = 0;
                this.curChar = '\0';
            }
            else
            {
                this.len = this.line.Length;
                this.NextChar();
            }
        }

    }
}
