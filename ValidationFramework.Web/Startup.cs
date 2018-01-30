using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ValidationFramework.Web.Startup))]

namespace ValidationFramework.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
           
        }
    }
}
