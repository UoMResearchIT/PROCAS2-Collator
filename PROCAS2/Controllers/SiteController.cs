using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Data;
using PROCAS2.Services.App;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;

namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class SiteController : Controller
    {
        private IGenericRepository<ScreeningSite> _siteRepo;

        private ISiteService _siteService;


        public SiteController(IGenericRepository<ScreeningSite> siteRepo,
                        ISiteService siteService)
        {
            _siteRepo = siteRepo;
            _siteService = siteService;
        }

        // GET: Site
        public ActionResult Index()
        {
            SiteListViewModel model = new SiteListViewModel();

            model.ScreeningSites = _siteRepo.GetAll().ToList();

            model.SiteNumbers = _siteService.ReturnAllSiteNumbers();

            return View("Index", model);
        }

        // GET: Site/Details/5
        public ActionResult Details(string code)
        {
            SiteEditViewModel model = new SiteEditViewModel();

            model = _siteService.FillEditViewModel(code);

            return View("Details", model);
        }

        // GET: Site/Create
        public ActionResult Create()
        {
            SiteEditViewModel model = new SiteEditViewModel();

            return View("Create", model);
        }

        // POST: Site/Create
        [HttpPost]
        public ActionResult Create(SiteEditViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _siteService.SaveSiteRecord(model);
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Create", model);
                }

            }
            catch
            {
                return View("Create", model);
            }
        }

        // GET: Site/Edit/5
        public ActionResult Edit(string code)
        {
            SiteEditViewModel model = new SiteEditViewModel();

            model = _siteService.FillEditViewModel(code);


            return View("Edit", model);
        }

        // POST: Site/Edit/5
        [HttpPost]
        public ActionResult Edit(SiteEditViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _siteService.SaveSiteRecord(model);
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Edit", model);
                }
                
            }
            catch
            {
                return View("Edit", model);
            }
        }

        
        // POST: Site/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string code)
        {
            bool del = false;
            if (String.IsNullOrEmpty(code) == false)
            {
                if (_siteService.DeleteSite(code) == true)
                {
                    del = true;
                }
            }

            return Json(new { deleted = del, code = code });
        }
    }
}
