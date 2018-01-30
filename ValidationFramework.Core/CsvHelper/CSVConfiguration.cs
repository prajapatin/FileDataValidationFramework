using System;
using System.Collections.Generic;

namespace ValidationFramework.Core.CsvHelper
{
    /// <summary>
    /// CSV file configuration class
    /// </summary>
    public class CSVConfiguration
    {
        public string Header { get; set; }
        public char FieldSeparator { get; set; }
        public char TextQualifier { get; set; }
        public IEnumerable<String> Columns { get; set; }
        public string EndOfLine { get; set; }

        public CSVConfiguration()
        {
            EndOfLine = "\r\n";
            FieldSeparator = ',';
            TextQualifier = '"';
          
        }
    }
}
