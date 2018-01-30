using ValidationFramework.Core;
using System.Collections.Generic;

namespace ValidationFramework.Core.Interfaces
{
    /// <summary>
    /// Interface to be implemented by file validator, which will validate existance of file, extension of file etc.
    /// </summary>
    public interface IFileValidator
    {
        IEnumerable<FileValidationError> Validate<T>(string filePath) where T : BaseEntity, new();
    }
}
