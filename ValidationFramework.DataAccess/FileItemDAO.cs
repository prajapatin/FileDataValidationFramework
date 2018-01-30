using ValidationFramework.BusinessEntities;
using System;
using System.Collections.Generic;

namespace ValidationFramework.DataAccess
{
    public class FileItemDAO : IFileItemDAO
    {
        public int SaveFile(FileItem fileItem)
        {
            //Enterprise library block to save object in database based on db configuration in web.config
            //var dbObject = DatabaseFactory.CreateDatabase();


            throw new NotImplementedException();
        }

        public int SaveFiles(List<FileItem> fileItems)
        {
            //Enterprise library block to save object in database based on db configuration in web.config
            //var dbObject = DatabaseFactory.CreateDatabase();
            throw new NotImplementedException();
        }
    }
}
