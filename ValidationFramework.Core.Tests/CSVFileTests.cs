using Microsoft.VisualStudio.TestTools.UnitTesting;
using ValidationFramework.BusinessEntities;
using ValidationFramework.Core.CsvHelper;
using System;
using System.Collections.Generic;

namespace ValidationFramework.Core.Tests
{
    [TestClass]
    public class CSVFileTests
    {
        [TestMethod]
        public void SimpleRead_Test()
        {
            List<Shipment> shipments = new List<Shipment>();
            var csvFileReader = new CSVFileReader<Shipment>("Shipment_Input.csv");
            foreach (Shipment shipment in csvFileReader)
            {
                shipments.Add(shipment);
            }
            Assert.IsNotNull(shipments);
            Assert.AreEqual(shipments.Count + csvFileReader.InvalidEntities.Count, 14);
        }

        [TestMethod]
        public void SimpleWrite_Test()
        {
            List<Shipment> outputData = new List<Shipment>();
            List<Shipment> inputData = new List<Shipment>();
            using (var csvFile = new CSVFile<Shipment>("Shipment_Output.csv"))
            {
                for (int i = 0; i < 10; i++)
                {
                    var shipment = new Shipment() { PickupDate = DateTime.Now, ShipmentOrigin = "Origin-" + i, ShipmentDestination = "Destination" + i};
                    outputData.Add(shipment);
                    csvFile.Append(shipment);
                }
            }
            var csvFileReader = new CSVFileReader<Shipment>("Shipment_Output.csv");
            foreach (Shipment shipment in csvFileReader)
            {
                inputData.Add(shipment);
            }
           
            Assert.AreEqual(outputData.Count, 10);
            Assert.AreEqual(inputData.Count, 10);
        }

        [TestMethod]
        public void ErrorEntity_Write_Test()
        {
            //No assertion for this test case, needs enhancements

            var csvConfig = new CSVConfiguration();
            List<string> columns = new List<string>();
            columns.Add("Shipment Origin");
            columns.Add("Shipment Destination");
            columns.Add("Pickup Date");
            columns.Add("Description");

            csvConfig.Columns = columns.ToArray();
           
            using (var csvFile = new CSVFile<ErrorEntity>("Shipment_Error_Output.csv", csvConfig))
            {
                for (int i = 0; i < 10; i++)
                {
                    var errorEntity = new ErrorEntity();
                    errorEntity.Description = "test error" + i;
                    errorEntity.EntityProperties = new EntityPropertyCollection();
                    errorEntity.EntityProperties.Add(new EntityProperty { PropertyName = "ShipmentOrigin", PropertyType = typeof(string), PropertyValue = "Origin_" + i });
                    errorEntity.EntityProperties.Add(new EntityProperty { PropertyName = "ShipmentDestination", PropertyType = typeof(string), PropertyValue = "Destination_" + i });
                    errorEntity.EntityProperties.Add(new EntityProperty { PropertyName = "PickupDate", PropertyType = typeof(DateTime), PropertyValue = DateTime.Now});

                    csvFile.Append(errorEntity);
                }
            }
            
        }
    }

   

}
