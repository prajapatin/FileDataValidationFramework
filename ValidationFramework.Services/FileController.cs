using ValidationFramework.BusinessEntities;
using ValidationFramework.BusinessLogic;
using ValidationFramework.Core;
using ValidationFramework.Core.Interfaces;
using ValidationFramework.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace ValidationFramework.Web.Controllers
{
    /// <summary>
    /// File service to be consumed by client to initiate file validation framework
    /// </summary>

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FileController : ApiController
    {
        [ImportMany]
        IEnumerable<Lazy<IFileValidator>> _fileValidators;

        [ImportMany]
        IEnumerable<Lazy<IFileFormatValidator<BaseEntity>>> _fileFormatValidators;

        [ImportMany]
        IEnumerable<Lazy<IBusinessRuleValidator<BaseEntity>>> _businessRuleValidators;

        /// <summary>
        /// Service method to process Shipment type of base entity
        /// </summary>
        /// <param name="fileModel">File model containing list of file paths to be processed for validation</param>
        /// <returns>File validation processing result</returns>
        [HttpPost]
        [Route("api/file")]
        public async Task<ProcessFileResultModel> ProcessFiles(ProcessFileModel fileModel)
        {
            try
            {
                FileFacade fileFacede = new FileFacade(_fileValidators, _fileFormatValidators, _businessRuleValidators);
                string folder = System.Web.Hosting.HostingEnvironment.MapPath("~/CSVFiles/");
                List<string> serverFilePaths = new List<string>();
                foreach (string filePath in fileModel.FilePaths)
                {
                    serverFilePaths.Add(folder + filePath);
                }
                var fileResultTasks = fileFacede.ProcessFiles<Shipment>(serverFilePaths.ToArray());

                var fileResults = await Task.WhenAll(fileResultTasks);
                return new ProcessFileResultModel { Message = "File processing successful", FileResults = fileResults };
            }
            catch
            {
                //ToDo: Proper logging at business logic level
                return new ProcessFileResultModel { Message = "Error in processing file/s!" };
            }
        }

        [HttpPost]
        [Route("api/revalidate/business")]
        public async Task<ProcessFileResultModel> OmitAndIgnoreBusinessValidation(ProcessFileModel fileModel)
        {
            try
            {
                FileFacade fileFacede = new FileFacade(_fileValidators, _fileFormatValidators, _businessRuleValidators);
                return await fileFacede.OmitAndIgnoreBusinessValidation<Shipment>(fileModel.FilePaths[0]).ContinueWith((resultTask) =>
                {
                    var fileResults = new List<FileResult>();
                    fileResults.Add(resultTask.Result);
                    return new ProcessFileResultModel { Message = "File processing successful", FileResults = fileResults.ToArray() };
                });
            }
            catch
            {
                //ToDo: Proper logging at business logic level
                return new ProcessFileResultModel { Message = "Error in processing file/s!" };
            }

        }

        [HttpPost]
        [Route("api/revalidate/format")]
        public async Task<ProcessFileResultModel> OmitAndIgnoreFormatValidation(ProcessFileModel fileModel)
        {
            try
            {
                FileFacade fileFacede = new FileFacade(_fileValidators, _fileFormatValidators, _businessRuleValidators);
                return await fileFacede.OmitAndIgnoreFormatValidation<Shipment>(fileModel.FilePaths[0]).ContinueWith((resultTask) =>
                {
                    var fileResults = new List<FileResult>();
                    fileResults.Add(resultTask.Result);
                    return new ProcessFileResultModel { Message = "File processing successful", FileResults = fileResults.ToArray() };
                });
            }
            catch
            {
                //ToDo: Proper logging at business logic level
                return new ProcessFileResultModel { Message = "Error in processing file/s!" };
            }

        }

        [HttpPost]
        [Route("api/file/download")]
        public HttpResponseMessage GetFile(ProcessFileModel fileModel)
        {
            FileFacade fileFacede = new FileFacade();
            //Here we know that all files are in single folder so we are sending only one folder path for Zip creation
            //Current service method will support zip file creation from diffrent paths as well
            List<string> filePaths = new List<string>();
            filePaths.Add(fileModel.FilePaths[0]);
            string filePath = fileFacede.CreateZip(filePaths);
            FileInfo fileInfo = new FileInfo(filePath);
            return FileAsAttachment(filePath, fileInfo.Name);
        }
        private HttpResponseMessage FileAsAttachment(string path, string filename)
        {
            if (File.Exists(path))
            {
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                var stream = new FileStream(path, FileMode.Open);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = filename;
                var cookie = new CookieHeaderValue("fileDownload", "true");
                cookie.Domain = Request.RequestUri.Host;
                cookie.Path = "/";
                result.Headers.AddCookies(new CookieHeaderValue[] { cookie });
                return result;
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}