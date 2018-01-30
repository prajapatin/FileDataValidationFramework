using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace ValidationFramework.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFiles(IEnumerable<HttpPostedFileBase> files)
        {
            string folder = Server.MapPath("~/CSVFiles/");
            List<string> filePaths = new List<string>();
            foreach (HttpPostedFileBase file in files)
            {
                string fileFolderUniqueKey = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss.fff", CultureInfo.InvariantCulture); 
                var directoryInfo = Directory.CreateDirectory(Path.Combine(folder, fileFolderUniqueKey));
                string filePath = Path.Combine(fileFolderUniqueKey, file.FileName);
                filePaths.Add(filePath);
                System.IO.File.WriteAllBytes(Path.Combine(directoryInfo.FullName, file.FileName), ReadData(file.InputStream));
            }
            return Json(new
            {
                isSuccess = true,
                files = filePaths.ToArray(),
                message = filePaths.Count == 1 ? "The file has been successfully uploaded." : "All files have been successfully uploaded."
            });
        }

        private byte[] ReadData(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}
