using ValidationFramework.BusinessEntities;
using System.Collections.Generic;

namespace ValidationFramework.Models
{
    public class ProcessFileModel
    {
        public List<string> FilePaths { get; set; }
    }

    public class ProcessFileResultModel
    {
        public string Message { get; set; }

        public FileResult[] FileResults { get; set; }
    }
}