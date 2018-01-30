using ValidationFramework.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationFramework.DataAccess
{
    public interface IFileItemDAO
    {
        int SaveFile(FileItem fileItem);
        int SaveFiles(List<FileItem> fileItems);
    }
}
