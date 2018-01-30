using ValidationFramework.BusinessLogic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace ValidationFramework.Web
{
    public class MEFConfig
    {
        public static void Configure()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(FileFacade).Assembly));
            ////catalog.Catalogs.Add(new DirectoryCatalog(@"..\\..\\ValidationPlugins")); // any specific directory
            var container = new CompositionContainer(catalog, true);
            container.ComposeParts();

            var currentWebApiActivator = GlobalConfiguration.Configuration.Services.GetHttpControllerActivator();
            var mefFactory = new MEFControllerFactory(container, currentWebApiActivator);
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), mefFactory);
        }
    }
}