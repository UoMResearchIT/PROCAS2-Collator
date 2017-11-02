using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Services.Utility;
using PROCAS2.Models.ViewModels;


namespace PROCAS2.Controllers
{
    [Authorize]
    public class UserAdminController : Controller
    {
        
        private IPROCAS2UserManager _procas2UserManager;
        private IContextService _contextService;
        private IGenericRepository<AppUser> _appUserRepo;

        public UserAdminController(IContextService contextService,
            IPROCAS2UserManager procas2UserManager,
            IGenericRepository<AppUser> appUserRepo)
        {
            _contextService = contextService;
            _procas2UserManager = procas2UserManager;
            _appUserRepo = appUserRepo;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            
        }

        // GET: UserAdmin
        public ActionResult Index()
        {
            UserAdminIndexViewModel model = new UserAdminIndexViewModel();

            model.AppUsers = _procas2UserManager.GetAllAppUsers();
         
            return View("Index", model);
        }

        [HttpGet]
        public ActionResult Suspend(int userId, bool flag)
        {
            _procas2UserManager.Suspend(userId, flag);

            return RedirectToAction("Index", "UserAdmin");
        }

        [HttpGet]
        public ActionResult SuperUser(int userId, bool flag)
        {

            _procas2UserManager.SuperUser(userId, flag);

            

            return RedirectToAction("Index", "UserAdmin");
        }

        

        // GET: UserAdmin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserAdmin/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

       
    }
}
