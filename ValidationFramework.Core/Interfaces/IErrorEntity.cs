using System;
using System.Collections.Generic;

namespace ValidationFramework.Core.Interfaces
{
    /// <summary>
    /// Interface for common error entity
    /// </summary>
    public interface IErrorEntity : IBaseEntity
    {
        IEntityPropertyCollection EntityProperties { get; set; }
        
    }

    /// <summary>
    /// Interface for entity prperty 
    /// </summary>
    public interface IEntityProperty
    {
        string PropertyName { get; set; }
        string PropertyHeader { get; set; }
        object PropertyValue { get; set; }

        Type PropertyType { get; set; }
    }

    /// <summary>
    /// Interface for entity property collection
    /// </summary>
    public interface IEntityPropertyCollection : IList<EntityProperty>
    {
        EntityProperty this[string propertyName] { get; set; }
    }
}
