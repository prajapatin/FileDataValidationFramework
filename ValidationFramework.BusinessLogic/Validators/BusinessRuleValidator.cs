using ValidationFramework.BusinessEntities;
using ValidationFramework.Core;
using ValidationFramework.Core.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace ValidationFramework.BusinessLogic.Validators
{
    /// <summary>
    /// Implementation of BR Validator, which would be injected dynamically in validation framework
    /// </summary>
    /// <typeparam name="T">Any object type, which inherits from BaseEntity</typeparam>
    [Export(typeof(IBusinessRuleValidator<>))]
    public class BusinessRulValidator<T> : IBusinessRuleValidator<T> where T : BaseEntity
    {
        public void Validate(IEnumerable<T> validEntities)
        {
            if (validEntities != null && validEntities.Any())
            {
                List<string> validServiceTypes = new List<string>() { "Standard" };
                List<string> validOriginDestinationCodes = new List<string>() { "ABL", "CGO", "CCI", "CLV", "CMB", "CRD" };
                foreach (var shipment in validEntities)
                {
                    Shipment concreteEntity = shipment as Shipment;

                    if (concreteEntity != null)
                    {
                        StringBuilder description = new StringBuilder();
                        if (concreteEntity.ShipmentOrigin == concreteEntity.ShipmentDestination)
                        {
                            description.Append("Origin and Destination cannot be same;");
                        }

                        if (!validServiceTypes.Contains(concreteEntity.ServiceType))
                        {
                            description.Append("Invalid service type value;");
                        }

                        if (!validOriginDestinationCodes.Contains(concreteEntity.ShipmentOrigin))
                        {
                            description.Append("Invalid origin code;");
                        }

                        if (!validOriginDestinationCodes.Contains(concreteEntity.ShipmentDestination))
                        {
                            description.Append("Invalid destination code;");
                        }

                        concreteEntity.Description = description.ToString();
                    }
                }
            }
        }
    }
}
