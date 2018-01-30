using ValidationFramework.Core;
using System;

namespace ValidationFramework.BusinessEntities
{
    public class Shipment : BaseEntity
    {
        public string ShipmentOrigin { get; set; }
        public string ShipmentDestination { get; set; }
        public DateTime PickupDate { get; set; }
        public int Weight { get; set; }
        public int Cube { get; set; }
        public string ServiceType { get; set; }
    }
}
