﻿using System;
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
               name: "HistologyEdit",
               url: "Histology/Edit/{participantId}/{primary}",
               defaults: new { controller = "Histology", action = "Edit", participantId = "", primary = 0 }
           );

            routes.MapRoute(
              name: "HistologyDelete",
              url: "Histology/Delete/{NHSNumber}/{primary}",
              defaults: new { controller = "Histology", action = "Delete", NHSNumber = "", primary = 0 }
          );

            routes.MapRoute(
              name: "HistologyFocusDelete",
              url: "Histology/DeleteFocus/{focusId}",
              defaults: new { controller = "Histology", action = "DeleteFocus", focusId = 0 }
          );

            routes.MapRoute(
             name: "HistologyFocusEdit",
             url: "Histology/EditFocus/{nhsnumber}/{headerId}/{primary}/{focusId}",
             defaults: new { controller = "Histology", action = "EditFocus", nhsnumber = "", headerId = 0, primary = 0, focusId = 0 }
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
              name: "ParticipantDelete",
              url: "Participant/Delete/{id}",
              defaults: new { controller = "Participant", action = "Delete", id = "" }
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
             name: "ParticipantUploadOutcomes",
             url: "Participant/UploadScreeningOutcomes/",
             defaults: new { controller = "Participant", action = "UploadScreeningOutcomes" }
         );

            routes.MapRoute(
               name: "SiteEdit",
               url: "Site/Edit/{code}",
               defaults: new { controller = "Site", action = "Edit", code = "" }
           );

            routes.MapRoute(
               name: "SiteDetails",
               url: "Site/Details/{code}",
               defaults: new { controller = "Site", action = "Details", code = "" }
           );

            routes.MapRoute(
              name: "TxErrorDetails",
              url: "TxErrors/Details/{id}",
              defaults: new { controller = "TxErrors", action = "Details",id =0 }
          );

            routes.MapRoute(
             name: "TxErrorReview",
             url: "TxErrors/Review/{id}",
             defaults: new { controller = "TxErrors", action = "Review", id = 0 }
         );

            routes.MapRoute(
              name: "SiteDelete",
              url: "Site/Delete/{code}",
              defaults: new { controller = "Site", action = "Delete", code = "" }
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
           name: "ViewQuestionnaire",
           url: "Questionnaire/Details/{responseId}",
           defaults: new { controller = "Questionnaire", action = "Details", responseId = "" }
       );

            routes.MapRoute(
            name: "ViewScreenV1_5_4",
            url: "Screening/ViewScreenV1_5_4/{id}",
            defaults: new { controller = "Screening", action = "ViewScreenV1_5_4", id = "" }
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
