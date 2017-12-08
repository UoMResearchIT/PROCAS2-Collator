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
                name: "RemoveIdentity",
                url: "UserAdmin/RemoveIdentity/{userId}",
                defaults: new { controller = "UserAdmin", action = "RemoveIdentity", userId = ""}
            );


            routes.MapRoute(
                name: "ParticipantEdit",
                url: "Participant/Edit/{participantId}",
                defaults: new { controller = "Participant", action = "Edit", participantId = "" }
            );

            routes.MapRoute(
               name: "ParticipantDetails",
               url: "Participant/Details/{participantId}",
               defaults: new { controller = "Participant", action = "Details", participantId = "" }
           );

            routes.MapRoute(
               name: "ParticipantUploadNew",
               url: "Participant/UploadNew/",
               defaults: new { controller = "Participant", action = "UploadNew"}
           );

            routes.MapRoute(
               name: "ParticipantUploadUpdate",
               url: "Participant/UploadUpdate/",
               defaults: new { controller = "Participant", action = "UploadUpdate" }
           );

            routes.MapRoute(
             name: "ExportLetters",
             url: "Export/",
             defaults: new { controller = "Export", action = "Export" }
         );

            routes.MapRoute(
              name: "ViewLetter",
              url: "Export/ViewLetter/{letterId}",
              defaults: new { controller = "Export", action = "ViewLetter", letterId = "" }
          );

            routes.MapRoute(
            name: "ViewScreenV1_5_2",
            url: "Screening/ViewScreenV1_5_2/{screenId}",
            defaults: new { controller = "Screening", action = "ViewScreenV1_5_2", screenId = "" }
        );

            routes.MapRoute(
              name: "ConsentPanel",
              url: "Home/ConsentPanel/",
              defaults: new { controller = "Home", action = "ConsentPanel" }
          );

            routes.MapRoute(
             name: "RiskPanel",
             url: "Home/RiskPanel/",
             defaults: new { controller = "Home", action = "RiskPanel" }
         );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
