using ValidationFramework.BusinessEntities;
using ValidationFramework.Core;
using ValidationFramework.Core.CsvHelper;
using ValidationFramework.Core.Interfaces;
using ValidationFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace ValidationFramework.BusinessLogic
{
    /// <summary>
    /// File validation processing facade
    /// </summary>
    public class FileFacade
    {

        IEnumerable<Lazy<IFileValidator>> _fileValidators;

        IEnumerable<Lazy<IFileFormatValidator<BaseEntity>>> _fileFormatValidators;

        IEnumerable<Lazy<IBusinessRuleValidator<BaseEntity>>> _businessRuleValidators;

        /// <summary>
        /// Constructor accepting dependencies for validators, which are lazyly initialized using Microsft MEF
        /// </summary>
        /// <param name="fileValidators">Lazily initialized file validators</param>
        /// <param name="fileFormatValidators">Lazily initialized file format validators</param>
        /// <param name="businessRuleValidators">Lazily initialized business rule validators for CSV file for specific type T</param>
        public FileFacade(IEnumerable<Lazy<IFileValidator>> fileValidators, IEnumerable<Lazy<IFileFormatValidator<BaseEntity>>> fileFormatValidators, IEnumerable<Lazy<IBusinessRuleValidator<BaseEntity>>> businessRuleValidators)
        {
            this._fileValidators = fileValidators;
            this._fileFormatValidators = fileFormatValidators;
            this._businessRuleValidators = businessRuleValidators;
        }

        /// <summary>
        /// Parameter-less constructor
        /// </summary>
        public FileFacade()
        {

        }

        /// <summary>
        /// Processes multiple files for validation and sends to data access layer if all validations are passed
        /// </summary>
        /// <typeparam name="T">Any type which inherits from validation framework base entity</typeparam>
        /// <param name="filePaths">File paths for CSV files</param>
        /// <returns>List of FileResult object</returns>
        public IEnumerable<Task<FileResult>> ProcessFiles<T>(params string[] filePaths) where T : BaseEntity, new()
        {
            IEnumerable<Task<FileResult>> processFileTasks = null;
            if (filePaths != null && filePaths.Length > 0)
            {
                processFileTasks = filePaths.Select(file => ProcessFile<T>(file));

            }
            return processFileTasks;
        }

        /// <summary>
        /// Method will persist file to backend and ignore any business rule validation
        /// </summary>
        /// <typeparam name="T">Any type which inherits from validation framework base entity</typeparam>
        /// <param name="filePath">File path, which requires re-processing</param>
        /// <returns>FileResult object</returns>
        public async Task<FileResult> OmitAndIgnoreBusinessValidation<T>(string filePath) where T : BaseEntity, new()
        {
            List<T> finalOutput = null;
            FileInfo fileInfo = new FileInfo(filePath);
            string fileToBeReProcessed = Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(filePath) + "_Output.csv");
            var csvFileReader = new CSVFileReader<T>(fileToBeReProcessed);
            List<T> entities = new List<T>();
            foreach (var entity in csvFileReader)
            {
                entities.Add(entity);
            }

            return await ProcessFinalOutput(filePath, fileInfo, entities, new List<string>(csvFileReader.CSVColumns), true).ContinueWith((finalRecords) =>
            {
                finalOutput = finalRecords.Result;

                //Persitance logic
                //FileItem fileItem = new FileItem { FileName = Path.GetFileNameWithoutExtension(filePath), FileType = typeof(T).FullName, FileData = File.ReadAllBytes(fileToBeReProcessed) };
                //DAOFactory.GetFileItemDAO().SaveFile(fileItem);

            }).ContinueWith((t) =>
            {
                return new FileResult
                {
                    FileName = Path.GetFileNameWithoutExtension(fileToBeReProcessed),
                    FilePath = fileInfo.FullName,
                    NumberOfValidRows = entities.Count,
                    NumberOfErrorRows = 0,
                    TypeOfError = "No Error"
                };
            });
        }

        /// <summary>
        /// Method will proceed ahead with further validation for valid data and ignore invalid records
        /// </summary>
        /// <typeparam name="T">Any type which inherits from validation framework base entity</typeparam>
        /// <param name="filePath">File path, which requires re-processing</param>
        /// <returns>FileResult object</returns>
        public async Task<FileResult> OmitAndIgnoreFormatValidation<T>(string filePath) where T : BaseEntity, new()
        {
            List<T> businessValidationErrors = null;
            List<T> finalOutput = null;
            FileInfo fileInfo = new FileInfo(filePath);

            string fileToBeReProcessed = Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(filePath) + "_Output.csv");
            List<T> entities = new List<T>();
            var csvFileReader = new CSVFileReader<T>(fileToBeReProcessed);
            foreach (var entity in csvFileReader)
            {
                entities.Add(entity);
            }

            var processTask = await ProcessBusinessValidation<T>(filePath, fileInfo, entities, new List<string>(csvFileReader.CSVColumns)).ContinueWith(async (validationErrors) =>
            {
                businessValidationErrors = await validationErrors;
                if (businessValidationErrors.Count != 0)
                {
                    await ProcessFinalOutput(filePath, fileInfo, entities, new List<string>(csvFileReader.CSVColumns), true).ContinueWith(async (finalRecords) =>
                    {
                        finalOutput = await finalRecords;

                        //Now we are having on-demand creation of Zip from UI
                        //CreateZip(filePath);

                    });
                }
                else
                {
                    //If file is passing buiness validation, we send it to data access layer for persistance
                    //FileItem fileItem = new FileItem { FileName = Path.GetFileNameWithoutExtension(filePath), FileType = typeof(T).FullName, FileData = File.ReadAllBytes(filePath) };
                    //DAOFactory.GetFileItemDAO().SaveFile(fileItem);

                }
            });

            return await processTask.ContinueWith((t) =>
             {
                 return new FileResult
                 {
                     FileName = Path.GetFileNameWithoutExtension(fileToBeReProcessed),
                     FilePath = fileInfo.FullName,
                     NumberOfValidRows = finalOutput != null ? finalOutput.Count : entities.Count,
                     NumberOfErrorRows = businessValidationErrors != null ? businessValidationErrors.Count : 0,
                     TypeOfError = businessValidationErrors != null && businessValidationErrors.Count > 0 ? "Business Validation" : "No Error"

                 };
             });

        }

        /// <summary>
        /// Creates Zip file from multiple source paths
        /// </summary>
        /// <param name="filePaths">File paths from which ZIP archive would be created</param>
        /// <returns>Path of Zip file</returns>
        public string CreateZip(List<string> filePaths)
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            string folder = System.Web.Hosting.HostingEnvironment.MapPath("~/CSVFiles/");
            string fileDirectory = string.Empty;
            foreach (string filePath in filePaths)
            {
                FileInfo fileInfo = new FileInfo(filePaths[0]);
                fileDirectory = fileInfo.Directory.Name;
                fileInfos.AddRange(fileInfo.Directory.GetFiles("*.*", SearchOption.TopDirectoryOnly)
             .Where(f => f.Extension.Equals(".csv", StringComparison.CurrentCultureIgnoreCase)).ToList());

            }

            //File zipping
            string zipFilePath = Path.Combine(Path.Combine(folder, fileDirectory), DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss.fff", CultureInfo.InvariantCulture) + ".zip");
            using (ZipArchive zipFile = ZipFile.Open(zipFilePath, new FileInfo(zipFilePath).Exists ? ZipArchiveMode.Update : ZipArchiveMode.Create))
            {
                foreach (var file in fileInfos)
                {
                    zipFile.CreateEntryFromFile(file.FullName, file.Name, CompressionLevel.Fastest);
                }
            }

            return zipFilePath;
        }
        private async Task<FileResult> ProcessFile<T>(string filePath) where T : BaseEntity, new()
        {
            //Following variables are not lazily initialized to avoid multiple time checking in case of many dynamic validators
            List<FileValidationError> fileValidationErrors = null;
            List<ErrorEntity> fileFormatErrors = null;
            List<T> businessValidationErrors = null;
            FileInfo fileInfo = new FileInfo(filePath);
            List<T> finalOutput = null;
            var csvFileReader = new CSVFileReader<T>(filePath);
            string[] columns = csvFileReader.CSVColumns.ToArray();
            List<string> entityColumns = new List<string>(columns);
            entityColumns.Add("Description");
            var processTask = await ProcessFileValidation<T>(filePath, fileInfo, entityColumns).ContinueWith(async (validationErrors) =>
            {

                fileValidationErrors = validationErrors.Result;
                if (fileValidationErrors.Count == 0)
                {
                    List<T> entities = new List<T>();
                    foreach (var entity in csvFileReader)
                    {
                        entities.Add(entity);
                    }

                    await ProcessFileFormatValidation<T>(filePath, fileInfo, entities, csvFileReader.InvalidEntities, entityColumns).ContinueWith((formatErrors) =>
                    {

                        fileFormatErrors = formatErrors.Result;

                    }).ContinueWith(async (t) =>
                    {
                        //If any file format errors, further validations will not happen
                        if (fileFormatErrors.Count == 0)
                        {
                            await ProcessBusinessValidation<T>(filePath, fileInfo, entities, entityColumns).ContinueWith((businessErrors) =>
                            {
                                businessValidationErrors = businessErrors.Result;

                            });
                        }
                        else
                        {
                            businessValidationErrors = new List<T>();
                        }
                    }).ContinueWith(async (t) =>
                    {
                        //In case of not file validation errors but there might be format errors or business validation errors, the valid records are still save as an output
                        finalOutput = await ProcessFinalOutput(filePath, fileInfo, entities, entityColumns);

                        //We are creating zip file if we find any one type of validation errors in file
                        if (fileValidationErrors.Count != 0 || fileFormatErrors.Count != 0 || businessValidationErrors.Count != 0)
                        {
                            //Now we are having on-demand creation of Zip from UI
                            //CreateZip(filePath);
                        }
                        else
                        {
                            //If file is passing all validations, we send it to data access layer for persistance
                            FileItem fileItem = new FileItem { FileName = Path.GetFileNameWithoutExtension(filePath), FileType = typeof(T).FullName, FileData = File.ReadAllBytes(filePath) };
                            DAOFactory.GetFileItemDAO().SaveFile(fileItem);
                        }
                    });
                }
                else
                {
                    fileFormatErrors = new List<ErrorEntity>();
                    businessValidationErrors = new List<T>();
                }
            });

            return await processTask.ContinueWith((t) =>
            {
                return new FileResult
                {
                    FileName = fileInfo.Name,
                    FilePath = fileInfo.FullName,
                    NumberOfValidRows = finalOutput != null ? finalOutput.Count : 0,
                    NumberOfErrorRows = fileValidationErrors.Count + fileFormatErrors.Count + businessValidationErrors.Count,
                    TypeOfError = fileValidationErrors.Count > 0 ? "File" : fileFormatErrors.Count > 0 ? "Format" : businessValidationErrors.Count > 0 ? "Business Validation" : "No Error"
                };
            });
        }

        private async Task<List<FileValidationError>> ProcessFileValidation<T>(string filePath, FileInfo fileInfo, List<string> columns) where T : BaseEntity, new()
        {
            List<FileValidationError> fileValidationErrors = new List<FileValidationError>();
            if (_fileValidators != null)
            {
                foreach (var fileValidator in _fileValidators)
                {
                    var validationErrors = fileValidator.Value.Validate<T>(filePath).ToList();
                    fileValidationErrors.AddRange(validationErrors);
                }
                if (fileValidationErrors.Count > 0)
                {
                    await fileValidationErrors.ToCsv(Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(filePath) + "_Error.csv"), new CSVConfiguration { Columns = columns });
                }
            }
            return fileValidationErrors;
        }

        private async Task<List<ErrorEntity>> ProcessFileFormatValidation<T>(string filePath, FileInfo fileInfo, List<T> entities, List<ErrorEntity> invalidEntities, List<string> entityColumns) where T : BaseEntity, new()
        {
            List<ErrorEntity> fileFormatErrors = new List<ErrorEntity>();
            //If any file validation errors, further validations will not happen
            if (_fileFormatValidators != null)
            {
                foreach (var fileFormatValidator in _fileFormatValidators)
                {
                    var formatErrors = fileFormatValidator.Value.Validate<T>(entities, invalidEntities);
                    if (formatErrors != null)
                    {
                        fileFormatErrors.AddRange(formatErrors);
                    }
                }
                if (fileFormatErrors.Count > 0)
                {
                    await fileFormatErrors.ToCsv(Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(filePath) + "Format_Error.csv"), new CSVConfiguration { Columns = entityColumns });
                }
            }
            return fileFormatErrors;
        }

        private async Task<List<T>> ProcessBusinessValidation<T>(string filePath, FileInfo fileInfo, List<T> entities, List<string> columns) where T : BaseEntity, new()
        {
            List<T> businessValidationErrors = new List<T>();
            if (_businessRuleValidators != null)
            {
                foreach (var businessRuleValidator in _businessRuleValidators)
                {
                    businessRuleValidator.Value.Validate(entities);
                    var validationErrors = entities.Where(x => !String.IsNullOrEmpty(x.Description));
                    if (validationErrors != null)
                    {
                        businessValidationErrors.AddRange(validationErrors);
                    }
                }
                if (businessValidationErrors.Count > 0)
                {
                    await businessValidationErrors.ToCsv(Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(filePath) + "Business_Error.csv"), new CSVConfiguration { Columns = columns });
                }
            }
            return businessValidationErrors;
        }

        private async Task<List<T>> ProcessFinalOutput<T>(string filePath, FileInfo fileInfo, List<T> entities, List<string> columns, bool isOmitAndContinue = false) where T : BaseEntity, new()
        {
            //In case of not file validation errors but there might be format errors or business validation errors, the valid records are still save as an output
            List<T> finalOutput = entities.Where(x => String.IsNullOrEmpty(x.Description)).ToList();
            if (finalOutput != null && finalOutput.Count > 0)
            {
                string file = Path.GetFileNameWithoutExtension(filePath);
                if (isOmitAndContinue)
                {
                    file = file + "_ErrorOmittedOutput.csv";
                }
                else
                {
                    file = file + "_Output.csv";
                }
                if (columns.Contains("Description"))
                {
                    columns.Remove("Description");
                }
                await finalOutput.ToCsv(Path.Combine(fileInfo.DirectoryName, file), new CSVConfiguration { Columns = columns });
            }
            return finalOutput;
        }
    }
}
