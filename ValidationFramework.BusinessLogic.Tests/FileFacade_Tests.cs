using Microsoft.VisualStudio.TestTools.UnitTesting;
using ValidationFramework.BusinessEntities;
using ValidationFramework.Core;
using ValidationFramework.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace ValidationFramework.BusinessLogic.Tests
{
    /// <summary>
    /// Summary description for FileFacade_Tests
    /// </summary>
    [TestClass]
    public class FileFacade_Tests
    {
        [ImportMany]
        IEnumerable<Lazy<IFileValidator>> _fileValidators;
        [ImportMany]
        IEnumerable<Lazy<IFileFormatValidator<BaseEntity>>> _fileFormatValidators;
        [ImportMany]
        IEnumerable<Lazy<IBusinessRuleValidator<BaseEntity>>> _businessRuleValidators;

        public FileFacade_Tests()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory));  // bin directory
            //catalog.Catalogs.Add(new DirectoryCatalog(@"..\\..\\ValidationPlugins")); // any specific directory

            CompositionContainer _container = new CompositionContainer(catalog);

            try
            {
                _container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            { }
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void ProcessFiles_InvalidFilePath_Test()
        {
            //No assertion for this test case, needs enhancements (currently verified manually)
            FileFacade fileFacade = new FileFacade(_fileValidators, _fileFormatValidators, _businessRuleValidators);
            fileFacade.ProcessFiles<Shipment>("Shipmnt_Input_Invalid.csv");
        }

        [TestMethod]
        public void ProcessFiles_InvalidFile_Test()
        {
            //No assertion for this test case, needs enhancements (currently verified manually)
            FileFacade fileFacade = new FileFacade(_fileValidators, _fileFormatValidators, _businessRuleValidators);
            fileFacade.ProcessFiles<Shipment>("Shipment_Input_Invalid.csv");
        }

        [TestMethod]
        public void ProcessFiles_InvalidFormat_Test()
        {
            //No assertion for this test case, needs enhancements (currently verified manually)
            FileFacade fileFacade = new FileFacade(_fileValidators, _fileFormatValidators, _businessRuleValidators);
            fileFacade.ProcessFiles<Shipment>("Shipment_Input_InvalidFormat.csv");
        }

        [TestMethod]
        public void ProcessFiles_InvalidBusiness_Test()
        {
            //No assertion for this test case, needs enhancements (currently verified manually)
            FileFacade fileFacade = new FileFacade(_fileValidators, _fileFormatValidators, _businessRuleValidators);
            fileFacade.ProcessFiles<Shipment>("Shipment_Input_BusinessInvalid.csv");
        }
    }
}
