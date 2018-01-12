using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;
using PROCAS2.Services.App;

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


        public HomeController(IUnitOfWork unitOfWork,
                                IGenericRepository<ScreeningSite> siteRepo,
                                IGenericRepository<AppNewsItem> appNewsItemsRepo,
                                IDashboardService dashboardService)
        {
            _unitOfWork = unitOfWork;
            _siteRepo = siteRepo;
            _appNewsItemsRepo = appNewsItemsRepo;
            _dashboardService = dashboardService;
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
            model.NumberYetToConsent = model.NumberParticipants - model.NumberConsented;
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

    }
}