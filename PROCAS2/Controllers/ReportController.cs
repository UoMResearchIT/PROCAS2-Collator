using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;

using PROCAS2.Models.ViewModels.Reports;
using PROCAS2.Services.App;
using PROCAS2.CustomActionResults;
using PROCAS2.Resources;

namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class ReportController : Controller
    {

        private ReportService _reportService;
        private ParticipantService _participantService;

        public ReportController(ReportService reportService,
                                ParticipantService participantService)
        {
            _reportService = reportService;
            _participantService = participantService;
        }

        // GET: Report
        public ActionResult Index()
        {
            return View("Index");
        }

        [HttpGet]
        public ActionResult PatientDetails()
        {
            PatientDetailsViewModel model = new PatientDetailsViewModel();

            return View("PatientDetails", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PatientDetails(PatientDetailsViewModel model)
        {
            if (ModelState.IsValid == true)
            {

                if (_participantService.DoesNHSNumberExist(model.NHSNumber) == false)
                {
                    ModelState.AddModelError("NHSNumber", ReportResources.NHS_NUMBER_NOT_EXIST);
                    return View("PatientDetails", model);
                }
               

                Response.Clear();

                var requestToken = Request.Cookies["fileDownloadToken"];
                if (requestToken != null && long.Parse(requestToken.Value) > 0)
                {
                    var responseTokenValue = long.Parse(requestToken.Value) * (-1);
                    Response.Cookies["fileDownloadToken"].Value = responseTokenValue.ToString();
                    Response.Cookies["fileDownloadToken"].Path = "/";
                }

                Response.Buffer = true;

               
                MemoryStream mStream = _reportService.PatientReport(new List<string>() { model.NHSNumber });

                string headerValue = string.Concat(1, ";Url=", PrependSchemeAndAuthority("Report/PatientDetails"));
                HttpContext.Response.AppendHeader("Refresh", headerValue);

                //TODO: set the sent risk flag

                return new SpreadsheetResult(mStream, "PatientDetails");


            }
            else
                return View("PatientDetails", model);



        }

        public string PrependSchemeAndAuthority(string url)
        {
            try
            {
                if (Request.Url.Authority.Contains("localhost"))
                {
                    return Request.Url.Scheme + "://"
                            + Request.Url.Authority + "/"
                            + url;
                }
                string urlBase = ConfigurationManager.AppSettings["UrlBase"];
                if (urlBase != null)
                {
                    return urlBase + "/" + url;
                }
                else
                {
                    return Request.Url.Scheme + "://"
                        + Request.Url.Authority + "/"
                        + url;
                }
            }
            catch
            {
                return url;
            }
        }
    }
}