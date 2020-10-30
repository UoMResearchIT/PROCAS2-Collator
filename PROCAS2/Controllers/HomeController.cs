using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;
using PROCAS2.Services.App;
using PROCAS2.Services.Utility;

namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class HomeController : Controller
    {

        private IUnitOfWork _unitOfWork;
        private IGenericRepository<ScreeningSite> _siteRepo;
        private IGenericRepository<AppNewsItem> _appNewsItemsRepo;
        private IDashboardService _dashboardService;
        private IWebJobLogger _logger;


        public HomeController(IUnitOfWork unitOfWork,
                                IGenericRepository<ScreeningSite> siteRepo,
                                IGenericRepository<AppNewsItem> appNewsItemsRepo,
                                IDashboardService dashboardService,
                                IWebJobLogger logger)
        {
            _unitOfWork = unitOfWork;
            _siteRepo = siteRepo;
            _appNewsItemsRepo = appNewsItemsRepo;
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public ActionResult Index()
        {

            return View();
        }

        public ActionResult ConsentPanel()
        {
            DashboardConsentViewModel model = new DashboardConsentViewModel();

            model.NumberConsented = _dashboardService.GetConsentedCount();
            model.NumberParticipants = _dashboardService.GetTotalParticipantCount() ;
            model.NumberConsentedNoDetails = _dashboardService.GetConsentedNoDetails();

            if (Request.IsAjaxRequest())
                return PartialView("_Consent", model);
            else
                return View("_Consent", model);
        }

        public ActionResult RiskPanel()
        {
            DashboardRiskViewModel model = new DashboardRiskViewModel();

            model.NumberLetterNotSent = _dashboardService.GetLetterNotSent();
            model.NumberWaitingForLetter = _dashboardService.GetWaitingForLetter();
            model.NumberLetterNotAskedFor = _dashboardService.GetRiskLetterNotAskedFor();
     

            if (Request.IsAjaxRequest())
                return PartialView("_RiskLetter", model);
            else
                return View("_RiskLetter", model);
        }

        public ActionResult SitePanel()
        {
            DashboardSiteViewModel model = new DashboardSiteViewModel();

            
            model.Sites = _siteRepo.GetAll().OrderBy(x => x.Code).ToList();

            if (Request.IsAjaxRequest())
                return PartialView("_Site", model);
            else
                return View("_Site", model);
        }

        public ActionResult AppNewsPanel()
        {
            DashboardAppNewsViewModel model = new DashboardAppNewsViewModel();


            model.NewsItems = _appNewsItemsRepo.GetAll().OrderByDescending(x => x.DatePosted).ToList();

            if (Request.IsAjaxRequest())
                return PartialView("_AppNews", model);
            else
                return View("_AppNews", model);
        }


        public ActionResult TxErrorsPanel()
        {
            DashboardTxErrorsViewModel model = new DashboardTxErrorsViewModel();

            model.CRAConsentErrors = _logger.GetLogCount(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning);
            model.CRASurveyErrors = _logger.GetLogCount(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning);
            model.VolparaMessageErrors = _logger.GetLogCount(WebJobLogMessageType.Volpara_Screening, WebJobLogLevel.Warning);
           

            if (Request.IsAjaxRequest())
                return PartialView("_TxErrors", model);
            else
                return View("_TxErrors", model);
        }

        public ActionResult VolparaPanel()
        {
            DashboardVolparaViewModel model = new DashboardVolparaViewModel();


            model.NumberLetterNotAskedFor = _dashboardService.GetRiskLetterNotAskedFor();
            model.NumberWaitingForVolpara = _dashboardService.GetWaitingForVolpara();
            model.NumberWaitingForVolparaNear6Weeks = _dashboardService.GetWaitingForVolparaNear6Weeks();

            if (Request.IsAjaxRequest())
                return PartialView("_Volpara", model);
            else
                return View("_Volpara", model);
        }


    }
}