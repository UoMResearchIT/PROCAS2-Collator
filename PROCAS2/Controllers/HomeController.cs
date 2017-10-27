using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Data;
using PROCAS2.Data.Entities;

namespace PROCAS2.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {

        private IUnitOfWork _unitOfWork;
        private IGenericRepository<ScreeningSite> _siteRepo;


        public HomeController(IUnitOfWork unitOfWork,
                                IGenericRepository<ScreeningSite> siteRepo)
        {
            _unitOfWork = unitOfWork;
            _siteRepo = siteRepo;
        }

        public ActionResult Index()
        {
            List<ScreeningSite> appUsers = _siteRepo.GetAll().ToList();
            return View();
        }

       
    }
}