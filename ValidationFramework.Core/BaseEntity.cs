using ValidationFramework.Core.CsvHelper;
using ValidationFramework.Core.Interfaces;

namespace ValidationFramework.Core
{
    /// <summary>
    /// Base entity to be inheritted by any entity which is to be used for validation framework for CSV
    /// </summary>
    public class BaseEntity : IBaseEntity
    {
        public BaseEntity()
        {

        }
        /// <summary>
        /// Description to be used for any kind of fault message for the entity
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Raw CSV text of current object
        /// </summary>
        [CsvIgnore]
        public string RowText { get; set; }
    }

}
