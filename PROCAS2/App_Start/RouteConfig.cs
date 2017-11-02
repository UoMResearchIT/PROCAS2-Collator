using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PROCAS2
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "SuspendUser",
                url: "UserAdmin/Suspend/{userId}/{flag}",
                defaults: new { controller = "UserAdmin", action = "Suspend", userId = "", flag = false }
            );

            routes.MapRoute(
                name: "SuperUser",
                url: "UserAdmin/SuperUser/{userId}/{flag}",
                defaults: new { controller = "UserAdmin", action = "SuperUser", userId = "", flag = false }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
