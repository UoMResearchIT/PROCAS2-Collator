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
using PROCAS2.Data;
using PROCAS2.Data.Entities;

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

        private IGenericRepository<ScreeningSite> _screeningSiteRepo;

        public ReportController(ReportService reportService,
                                ParticipantService participantService,
                                IGenericRepository<ScreeningSite> screeningSiteRepo)
        {
            _reportService = reportService;
            _participantService = participantService;
            _screeningSiteRepo = screeningSiteRepo;
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

            return NoParameterReport(() => _reportService.Histology(), "Histology", "NoParameter", model);

           

        }


        [HttpGet]
        public ActionResult Volpara()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.VOLPARA;
            model.Summary = ReportResources.VOLPARA_SUMMARY;
            model.ActionName = "Volpara";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Volpara(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.Volpara(), "Volpara", "NoParameter", model);

        }

        [HttpGet]
        public ActionResult Invited()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.INVITED;
            model.Summary = ReportResources.INVITED_SUMMARY;
            model.ActionName = "Invited";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Invited(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.Invited(), "Invited", "NoParameter", model);

        }

        [HttpGet]
        public ActionResult Consented()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.CONSENTED;
            model.Summary = ReportResources.CONSENTED_SUMMARY;
            model.ActionName = "Consented";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Consented(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.Consented(), "Consented", "NoParameter", model);

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

            return NoParameterReport(() => _reportService.YetToConsent(), "YetToConsent", "NoParameter", model);
           
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

            return NoParameterReport(() => _reportService.YetToGetFull(), "YetToGetFull", "NoParameter", model);

            
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

            return NoParameterReport(() => _reportService.YetToAskForRisk(), "YetToAskForRisk", "NoParameter", model);

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

            return NoParameterReport(() => _reportService.YetToReceiveLetter(), "YetToReceiveLetter", "NoParameter", model);

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
            if (ModelState.IsValid)
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

                MemoryStream askFile = _reportService.AskForRiskLetters();

                string headerValue = string.Concat(1, ";Url=", PrependSchemeAndAuthority("Report/AskForRiskLetters"));
                HttpContext.Response.AppendHeader("Refresh", headerValue);

                return new TextResult(askFile, "AskForRiskLetters.txt");
                
            }

            return View("AskForRiskLetters", model);
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

            return NoParameterReport(() => _reportService.YetToSendLetter(), "YetToSendLetter", "NoParameter", model);

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

            return NoParameterReport(() => _reportService.WaitingForVolpara(), "WaitingForVolpara", "NoParameter", model);

        }


        [HttpGet]
        public ActionResult WaitingForVolparaNear6Weeks()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.WAITING_FOR_VOLPARA_NEAR_6_WEEKS;
            model.Summary = ReportResources.WAITING_FOR_VOLPARA_NEAR_6_WEEKS_SUMMARY;
            model.ActionName = "WaitingForVolparaNear6Weeks";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WaitingForVolparaNear6Weeks(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.WaitingForVolparaNear6Weeks(), "WaitingForVolparaNear6Weeks", "NoParameter", model);

        }


        [HttpGet]
        public ActionResult ScreeningAttendance()
        {
            NoParameterTypeChoiceViewModel model = new NoParameterTypeChoiceViewModel();

            model.Title = ReportResources.SCREENING_ATTENDANCE;
            model.Summary = ReportResources.SCREENING_ATTENDANCE_SUMMARY;
            model.ActionName = "ScreeningAttendance";
            model.ReportType = "FIRST";

            return View("ScreeningAttendance", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ScreeningAttendance(NoParameterTypeChoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ReportType == "FIRST")
                {
                    return NoParameterReport(() => _reportService.ScreeningFirstOffered(), "ScreeningFirstOffered", "ScreeningAttendance", model);
                }
                else
                {
                    return NoParameterReport(() => _reportService.ScreeningWithin180Days(), "ScreeningWithin180Days", "ScreeningAttendance", model);
                }
            }
            else
            {
                return View("ScreeningAttendance", model);
            }

         

        }

        [HttpGet]
        public ActionResult SubsequentConsultation()
        {
            NoParameterTypeChoiceViewModel model = new NoParameterTypeChoiceViewModel();

            model.Title = ReportResources.SUBSEQUENT_CONSULTATION;
            model.Summary = ReportResources.SUBSEQUENT_CONSULTATION_SUMMARY;
            model.ActionName = "SubsequentConsultation";
            model.ReportType = "FAMHIST";

            return View("SubsequentConsultation", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubsequentConsultation(NoParameterTypeChoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ReportType == "FAMHIST")
                {
                    return NoParameterReport(() => _reportService.SubsequentFamilyHistory(), "SubsequentFamilyHistory", "SubsequentConsultation", model);
                }
                else
                {
                    return NoParameterReport(() => _reportService.SubsequentMoreFrequent(), "SubsequentMoreFrequent", "SubsequentConsultation", model);
                }
            }
            else
            {
                return View("SubsequentConsultation", model);
            }



        }


        [HttpGet]
        public ActionResult Chemoprevention()
        {
            NoParameterTypeChoiceViewModel model = new NoParameterTypeChoiceViewModel();

            model.Title = ReportResources.CHEMOPREVENTION;
            model.Summary = ReportResources.CHEMOPREVENTION_SUMMARY;
            model.ActionName = "Chemoprevention";
            model.ReportType = "DISAGREED";

            return View("Chemoprevention", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Chemoprevention(NoParameterTypeChoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ReportType == "DISAGREED")
                {
                    return NoParameterReport(() => _reportService.ChemoDisagreed(), "ChemoDisagreed", "Chemoprevention", model);
                }
                else if (model.ReportType == "NOTAPP")
                {
                    return NoParameterReport(() => _reportService.ChemoNotApp(), "ChemoNotApp", "Chemoprevention", model);
                }
                else if (model.ReportType == "NOTFILLED")
                {
                    return NoParameterReport(() => _reportService.ChemoNotFilled(), "ChemoNotFilled", "Chemoprevention", model);
                }
                else
                {
                    return NoParameterReport(() => _reportService.ChemoFilled(), "ChemoFilled", "Chemoprevention", model);
                }
            }
            else
            {
                return View("Chemoprevention", model);
            }



        }



        [HttpGet]
        public ActionResult NumberRecalls()
        {
            NoParameterTypeChoiceViewModel model = new NoParameterTypeChoiceViewModel();

            model.Title = ReportResources.NUMBER_RECALLS;
            model.Summary = ReportResources.NUMBER_RECALLS_SUMMARY;
            model.ActionName = "NumberRecalls";
            model.ReportType = "TECH";

            return View("NumberRecalls", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NumberRecalls(NoParameterTypeChoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ReportType == "TECH")
                {
                    return NoParameterReport(() => _reportService.NumberTechnicalRecalls(), "NumberTechnicalRecalls", "NumberRecalls", model);
                }
                else if (model.ReportType == "ASSESS")
                {
                    return NoParameterReport(() => _reportService.NumberAssessmentRecalls(), "NumberAssessmentRecalls", "NumberRecalls", model);
                }
                else
                {
                    return NoParameterReport(() => _reportService.NumberRoutineRecalls(), "NumberRoutineRecalls", "NumberRecalls", model);
                }
            }
            else
            {
                return View("NumberRecalls", model);
            }

        }

        [HttpGet]
        public ActionResult BreastCancerDiagnoses()
        {
            NoParameterViewModel model = new NoParameterViewModel();

            model.Title = ReportResources.BC_DIAGNOSES;
            model.Summary = ReportResources.BC_DIAGNOSES_SUMMARY;
            model.ActionName = "BreastCancerDiagnoses";

            return View("NoParameter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BreastCancerDiagnoses(NoParameterViewModel model)
        {

            return NoParameterReport(() => _reportService.BreastCancerDiagnoses(), "BreastCancerDiagnoses", "NoParameter", model);


        }

        private ActionResult NoParameterReport(Func<MemoryStream> reportFunction, string reportName, string viewName, object model)
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
                return View(viewName, model);

        }


        [HttpGet]
        public ActionResult QueryDatabase()
        {
            QueryDatabaseViewModel model = new QueryDatabaseViewModel();

            model.Anonymise = true;
            model.IncludeAddresses = false;
            model.IncludeHistology = false;
            model.OnlyMostRecentQuestionnaire = true;
            model.OnlyMostRecentRisk = true;
            model.OnlyMostRecentVolpara = true;
            model.ConsentedFrom = Convert.ToDateTime("01/01/2018");
            model.ConsentedTo = Convert.ToDateTime("01/01/2021");

            model.ScreeningSite = _screeningSiteRepo.GetAll().FirstOrDefault().Code;
            model.ScreeningSites = _screeningSiteRepo.GetAll().OrderBy(x => x.Name).ToList();

            return View("QueryDatabase", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QueryDatabase(QueryDatabaseViewModel model)
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


                MemoryStream mStream = _reportService.QueryDB(model);

                string headerValue = string.Concat(1, ";Url=", PrependSchemeAndAuthority("Report/QueryDatabase"));
                HttpContext.Response.AppendHeader("Refresh", headerValue);



                return new SpreadsheetResult(mStream, "CollatorDB");


            }
            else
            {
                model.ScreeningSites = _screeningSiteRepo.GetAll().OrderBy(x => x.Name).ToList();
                return View("QueryDatabase", model);
            }
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