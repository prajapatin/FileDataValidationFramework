using ValidationFramework.Core;
using ValidationFramework.Core.CsvHelper;
using ValidationFramework.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ValidationFramework.BusinessLogic.Validators
{
    /// <summary>
    /// Implementation of File Validator, which would be injected dynamically in validation framework
    /// </summary>
    [Export(typeof(IFileValidator))]
    public class FileValidator : IFileValidator
    {
        public IEnumerable<FileValidationError> Validate<T>(string filePath) where T : BaseEntity, new()
        {

            List<FileValidationError> fileValidationErrors = new List<FileValidationError>();
            FileInfo fileInfo = new FileInfo(filePath);
            
            if(!fileInfo.Exists)
            {
                fileValidationErrors.Add(new FileValidationError {ErrorType = "Error", Description = "File does not exist" });
            }

            if (fileInfo.Exists)
            {
                if (fileInfo.Length == 0)
                {
                    fileValidationErrors.Add(new FileValidationError { ErrorType = "Error", Description = "File is empty" });
                }

                if (fileInfo.Extension.ToLower() != ".csv")
                {
                    fileValidationErrors.Add(new FileValidationError { ErrorType = "Extenion Not Supported", Description = fileInfo.Extension });
                }

                var csvFileReader = new CSVFileReader<T>(filePath);
                string[] columns = csvFileReader.NormalizedColumns;
                
                if (!csvFileReader.HasRecords())
                {
                    fileValidationErrors.Add(new FileValidationError { ErrorType = "Only Header", Description = "File does not have data!" });
                }

                var flags = BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance;
                string[] entityProperties = typeof(T).GetProperties(flags).Select(x => x.Name).Where(x => !typeof(BaseEntity).GetProperties(flags).Select(y => y.Name).Contains(x)).ToArray();

                string columnsOnlyInFile = String.Join(",", columns.Except(entityProperties));
                if (!String.IsNullOrEmpty(columnsOnlyInFile))
                {
                    fileValidationErrors.Add(new FileValidationError { ErrorType = "Extra Columns", Description = columnsOnlyInFile });
                }

                string columnsOnlyInEntity = String.Join(",", entityProperties.Except(columns));
                if (!String.IsNullOrEmpty(columnsOnlyInFile))
                {
                    fileValidationErrors.Add(new FileValidationError { ErrorType = "Missing Columns", Description = columnsOnlyInEntity });
                }
            }

            return fileValidationErrors;
        }
    }
}
