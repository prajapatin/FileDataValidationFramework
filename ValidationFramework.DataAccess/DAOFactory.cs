using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationFramework.DataAccess
{
    public class DAOFactory
    {
        public static IFileItemDAO GetFileItemDAO()
        {
            return new FileItemDAO();
        }
    }
}
