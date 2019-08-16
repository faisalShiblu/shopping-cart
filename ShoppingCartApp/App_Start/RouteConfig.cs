using System.Web.Mvc;
using System.Web.Routing;

namespace ShoppingCartApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Account",
                          "Account/{action}/{id}",
                          new { controller = "Account", action = "Index", id = UrlParameter.Optional },
                          new[] { "ShoppingCartApp.Controllers" });

            routes.MapRoute("Cart",
                          "Cart/{action}/{id}",
                          new { controller = "Cart", action = "Index", id = UrlParameter.Optional },
                          new[] { "ShoppingCartApp.Controllers" });

            routes.MapRoute("Shop",
                           "Shop/{action}/{name}",
                           new { controller = "Shop", action = "Index", name = UrlParameter.Optional },
                           new[] { "ShoppingCartApp.Controllers" });

            routes.MapRoute("SidebarPartial",
                           "Pages/SidebarPartial",
                           new { controller = "Pages", action = "SidebarPartial" },
                           new[] { "ShoppingCartApp.Controllers" });

            routes.MapRoute("PagesMenuPartial",
                            "Pages/PagesMenuPartial",
                            new { controller = "Pages", action = "PagesMenuPartial" },
                            new[] { "ShoppingCartApp.Controllers" });

            routes.MapRoute("Pages",
                            "{page}",
                            new { controller = "Pages", action = "Index" },
                            new[] { "ShoppingCartApp.Controllers" });

            routes.MapRoute("Default",
                            "",
                            new { controller = "Pages", action = "Index" },
                            new[] { "ShoppingCartApp.Controllers" });

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
