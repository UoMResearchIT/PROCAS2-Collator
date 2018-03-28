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

               

                return new SpreadsheetResult(mStream, "PatientDetails");


            }
            else
                return View("PatientDetails", model);



        }


        [HttpGet]
        public ActionResult Histology()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.HISTOLOGY;
            model.Summary = ReportResources.HISTOLOGY_SUMMARY;
            model.ActionName = "Histology";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Histology(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.Histology(), "Histology", model);

           

        }

        [HttpGet]
        public ActionResult YetToConsent()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.INVITED_YET_TO_CONSENT;
            model.Summary = ReportResources.INVITED_YET_TO_CONSENT_SUMMARY;
            model.ActionName = "YetToConsent";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult YetToConsent(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.YetToConsent(), "YetToConsent", model);
           
        }

        [HttpGet]
        public ActionResult YetToGetFull()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.CONSENTED_YET_TO_GET_FULL;
            model.Summary = ReportResources.CONSENTED_YET_TO_GET_FULL_SUMMARY;
            model.ActionName = "YetToGetFull";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult YetToGetFull(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.YetToGetFull(), "YetToGetFull", model);

            
        }


        [HttpGet]
        public ActionResult YetToAskForRisk()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.GOT_VOLPARA_NOT_ASKED;
            model.Summary = ReportResources.GOT_VOLPARA_NOT_ASKED_SUMMARY;
            model.ActionName = "YetToAskForRisk";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult YetToAskForRisk(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.YetToAskForRisk(), "YetToAskForRisk", model);

        }


        [HttpGet]
        public ActionResult YetToReceiveLetter()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.ASKED_FOR_LETTER_NOT_REC;
            model.Summary = ReportResources.ASKED_FOR_LETTER_NOT_REC_SUMMARY;
            model.ActionName = "YetToReceiveLetter";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult YetToReceiveLetter(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.YetToReceiveLetter(), "YetToReceiveLetter", model);

        }

        [HttpGet]
        public ActionResult AskForRiskLetters()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.ASK_FOR_RISK_LETTER;
            model.Summary = ReportResources.ASK_FOR_RISK_LETTER_SUMMARY;
            model.ActionName = "AskForRiskLetters";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AskForRiskLetters(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.AskForRiskLetters(), "AskForRiskLetters", model);

        }

        [HttpGet]
        public ActionResult YetToSendLetter()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.GOT_LETTER_NOT_SENT;
            model.Summary = ReportResources.GOT_LETTER_NOT_SENT_SUMMARY;
            model.ActionName = "YetToSendLetter";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult YetToSendLetter(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.YetToSendLetter(), "YetToSendLetter", model);

        }

        [HttpGet]
        public ActionResult WaitingForVolpara()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.WAITING_FOR_VOLPARA;
            model.Summary = ReportResources.WAITING_FOR_VOLPARA_SUMMARY;
            model.ActionName = "WaitingForVolpara";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WaitingForVolpara(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.WaitingForVolpara(), "WaitingForVolpara", model);

        }


        private ActionResult NoParameterReport(Func<MemoryStream> reportFunction, string reportName, NoParameterViewModel model)
        {
            if (ModelState.IsValid == true)
            {



                Response.Clear();

                var requestToken = Request.Cookies["fileDownloadToken"];
                if (requestToken != null && long.Parse(requestToken.Value) > 0)
                {
                    var responseTokenValue = long.Parse(requestToken.Value) * (-1);
                    Response.Cookies["fileDownloadToken"].Value = responseTokenValue.ToString();
                    Response.Cookies["fileDownloadToken"].Path = "/";
                }

                Response.Buffer = true;


                MemoryStream mStream = reportFunction();

                string headerValue = string.Concat(1, ";Url=", PrependSchemeAndAuthority("Report/" + reportName));
                HttpContext.Response.AppendHeader("Refresh", headerValue);



                return new SpreadsheetResult(mStream, reportName);


            }
            else
                return View("NoParameter", model);

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