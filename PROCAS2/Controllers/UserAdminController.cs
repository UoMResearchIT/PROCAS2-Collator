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
using PROCAS2.Resources;
using PROCAS2.CustomAttributes;


namespace PROCAS2.Controllers
{
    
#if RELEASE
    [RequireHttps]
#endif
    [RedirectIfNotAuthorised(Roles = "Super")]
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
            UserAdminCreateViewModel model = new UserAdminCreateViewModel();

            model.Active = true;

            return View("Create", model);
        }

        // POST: UserAdmin/Create
        [HttpPost]
        public ActionResult Create(UserAdminCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AppUser appUser = _appUserRepo.GetAll().Where(x => x.UserCode == model.UserCode.ToLower()).FirstOrDefault();
                    if (appUser != null) // user already exists!
                    {
                        ModelState.AddModelError("UserCode", PROCASRes.USER_ALREADY_EXISTS );
                        return View("Create", model);
                    }
                    else
                    {
                        _procas2UserManager.AddUser(model.UserCode, model.SuperUser, model.Active);
                        return RedirectToAction("Index", "UserAdmin");
                    }
                }
                else
                {
                    return View("Create", model);
                }

                
            }
            catch
            {
                return View();
            }
        }

       
    }
}
