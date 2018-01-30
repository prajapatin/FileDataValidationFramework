using ValidationFramework.Core.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ValidationFramework.Core
{
    /// <summary>
    /// Error entity to be used for any of the entity type used in validation framework
    /// </summary>
    public class ErrorEntity : BaseEntity, IErrorEntity
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public ErrorEntity()
        {
            this.EntityProperties = new EntityPropertyCollection();
        }
      
        /// <summary>
        /// Property collection for the current error entity
        /// </summary>
        public IEntityPropertyCollection EntityProperties { get; set; }
    }

    /// <summary>
    /// Entity property indicating instance prperty of specific type
    /// </summary>
    public class EntityProperty : IEntityProperty
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Property's label name
        /// </summary>
        public string PropertyHeader { get; set; }

        /// <summary>
        /// Data type of the property
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Property value
        /// </summary>
        public object PropertyValue { get; set; }
    }

    /// <summary>
    /// Collection of EntityProperty type
    /// </summary>
    public class EntityPropertyCollection : IEntityPropertyCollection
    {
        List<EntityProperty> _items = null;

        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public EntityPropertyCollection()
        {
            _items = new List<EntityProperty>();
        }

        /// <summary>
        /// Constructor accepting IEnumerable entity property list
        /// </summary>
        /// <param name="items">Items using which entity property collection to be initialized</param>
        public EntityPropertyCollection(IEnumerable<EntityProperty> items)
        {
            _items = new List<EntityProperty>(items);
        }

        /// <summary>
        /// Provides access to specific entity property based name of entity property
        /// </summary>
        /// <param name="propertyName">Name of the entity property</param>
        /// <returns>EntityProperty instance</returns>
        public EntityProperty this[string propertyName]
        {

            get
            {
                EntityProperty requiredItem = null;
                foreach (EntityProperty property in this._items)
                {
                    if (property.PropertyName == propertyName)
                    {
                        requiredItem = property;
                        break;
                    }
                }
                return requiredItem;
            }

            set
            {
                throw new NotImplementedException();
            }

        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public EntityProperty this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Count of Entity properties
        /// </summary>
        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Adds EntityProperty to current collection
        /// </summary>
        /// <param name="item">Object to be added to collection</param>
        public void Add(EntityProperty item)
        {
            this._items.Add(item);
        }

        /// <summary>
        /// Clears all object from underlying list
        /// </summary>
        public void Clear()
        {
            this._items.Clear();
        }

        /// <summary>
        /// Checks whether provided entity property is available in list
        /// </summary>
        /// <param name="item">EntityProperty instance for which existance in collection is required</param>
        /// <returns>True if entity property exists in list otherwise False</returns>
        public bool Contains(EntityProperty item)
        {
            return this._items.Exists(x => x.PropertyName == item.PropertyName);
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(EntityProperty[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        public IEnumerator<EntityProperty> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(EntityProperty item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, EntityProperty item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(EntityProperty item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
