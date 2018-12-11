[assembly: Microsoft.Owin.OwinStartup(typeof(CheckoutAuthCodeGrant.Startup))]

namespace CheckoutAuthCodeGrant
{
    using Owin;
    using System.Web.Optimization;
    using System.Web.Routing;

    /// <summary>
    /// Initialize the web application.
    /// </summary>
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
