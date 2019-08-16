using ShoppingCartApp.Models.Data;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ShoppingCartApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest()
        {
            if (User == null)
                return;

            string username = Context.User.Identity.Name;

            string[] roles = null;

            using (CartDbContext db = new CartDbContext())
            {
                var dto = db.Users.FirstOrDefault(u => u.UserName == username);

                roles = db.UserRoles.Where(u => u.UserId == dto.Id).Select(u => u.Role.Name).ToArray();
            }

            IIdentity userIdentity = new GenericIdentity(username);
            IPrincipal newUserObject = new GenericPrincipal(userIdentity, roles);

            Context.User = newUserObject;
        }
    }
}
