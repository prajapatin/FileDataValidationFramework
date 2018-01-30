using System.Collections.Generic;

namespace ValidationFramework.Core.Interfaces
{
    /// <summary>
    /// Interface to be implemented by business rule validator for file validation
    /// </summary>
    /// <typeparam name="T">Any type implementing IBaseEntity interface</typeparam>
    public interface IBusinessRuleValidator<T> where T : IBaseEntity
    {
        void Validate(IEnumerable<T> validEntities);
    }
}
