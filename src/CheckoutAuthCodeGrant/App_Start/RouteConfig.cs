namespace CheckoutAuthCodeGrant
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Configure endpoint routes.
    /// </summary>
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();
        }
    }
}