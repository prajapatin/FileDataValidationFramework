using System.Collections.Generic;

namespace ValidationFramework.Core.Interfaces
{
    /// <summary>
    /// Interface to be used by any file format validator for validation framework
    /// </summary>
    /// <typeparam name="T">Any type implementing IBaseEntity interface</typeparam>
    public interface IFileFormatValidator<T> where T : IBaseEntity
    {
        IEnumerable<ErrorEntity> Validate<T>(IEnumerable<T> validEntities, IEnumerable<ErrorEntity> parsingErrorEntities);
    }
}
