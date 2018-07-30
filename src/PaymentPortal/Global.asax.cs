using PaymentPortal.Services;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PaymentPortal
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Register and Run Background Service if Provider Uses One
            var provider = ProviderService.GetProvider();
            var backgroundService = provider.GetHostedService();
            if (backgroundService != null)
            {
                HostingEnvironment.RegisterObject(backgroundService);
            }
        }
    }
}
