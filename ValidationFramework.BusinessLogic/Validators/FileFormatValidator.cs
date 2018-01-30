using ValidationFramework.Core;
using ValidationFramework.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ValidationFramework.BusinessLogic.Validators
{
    /// <summary>
    /// Implementation of CSV File format Validator, which would be injected dynamically in validation framework
    /// </summary>
    /// <typeparam name="T">Any object type, which inherits from BaseEntity</typeparam>
    [Export(typeof(IFileFormatValidator<>))]
    public class FileFormatValidator<T> : IFileFormatValidator<T> where T : BaseEntity
    {
        public IEnumerable<ErrorEntity> Validate<T>(IEnumerable<T> validEntities, IEnumerable<ErrorEntity> parsingErrorEntities)
        {
            if (parsingErrorEntities != null)
            {
                var flags = BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance;
                PropertyInfo[] entityProperties = typeof(T).GetProperties(flags);
               
                foreach(var errorEntity in parsingErrorEntities)
                {
                    StringBuilder errorDescription = new StringBuilder();
                    foreach(var propInfo in entityProperties)
                    {
                       var entityProperty = errorEntity.EntityProperties[propInfo.Name];
                        if (entityProperty != null)
                        {
                            if (entityProperty.PropertyValue == null || String.IsNullOrWhiteSpace(entityProperty.PropertyValue.ToString()))
                            {
                                errorDescription.AppendFormat("Null or Empty - {0};", entityProperty.PropertyHeader);
                            }
                            else
                            {
                                switch (propInfo.PropertyType.ToString())
                                {
                                    case "System.String":
                                        if(!Regex.IsMatch(entityProperty.PropertyValue.ToString(), @"^[a-zA-Z]+$"))
                                        {
                                            errorDescription.AppendFormat("{0}-{1} Data type;", entityProperty.PropertyHeader, propInfo.PropertyType);
                                        }
                                        break;
                                    case "System.Int32":
                                        Int32 outIntVal;
                                        if (!Int32.TryParse(entityProperty.PropertyValue.ToString(), out outIntVal))
                                        {
                                            errorDescription.AppendFormat("{0}-{1} Data type;", entityProperty.PropertyHeader, propInfo.PropertyType);
                                        }
                                        break;
                                    case "System.Int64":
                                        Int64 outLongVal;
                                        if (!Int64.TryParse(entityProperty.PropertyValue.ToString(), out outLongVal))
                                        {
                                            errorDescription.AppendFormat("{0}-{1} Data type;", entityProperty.PropertyHeader, propInfo.PropertyType);
                                        }
                                        break;
                                    case "System.DateTime":
                                        DateTime outDateTimeVal;
                                        if (!DateTime.TryParse(entityProperty.PropertyValue.ToString(), out outDateTimeVal))
                                        {
                                            errorDescription.AppendFormat("{0}-{1} Data type;", entityProperty.PropertyHeader, propInfo.PropertyType);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    errorEntity.Description = errorDescription.ToString().TrimEnd(';');
                    
                }
            }
            return parsingErrorEntities;

        }
    }
}
