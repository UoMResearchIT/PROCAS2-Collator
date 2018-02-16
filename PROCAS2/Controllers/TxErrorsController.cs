using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Services.Utility;
using PROCAS2.Models.ViewModels;

namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class TxErrorsController : Controller
    {

        private IWebJobLogger _logger;

        public TxErrorsController(IWebJobLogger logger)
        {
            _logger = logger;
        }

        // GET: TxErrors
        public ActionResult Index()
        {
            TxErrorsListViewModel model = new TxErrorsListViewModel();

            model.TxErrors = _logger.GetAllCurrentErrors();

            return View("Index", model);
        }

        // POST: TxErrors/Review/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Review(int id)
        {
            bool rev = false;
            if (id != 0)
            {
                if (_logger.Review(id) == true)
                {
                    rev = true;
                }
            }

            return Json(new { reviewed = rev });
        }

        public ActionResult Details(int id)
        {
            TxErrorsDetailsViewModel model = new TxErrorsDetailsViewModel();

            model = _logger.FillDetailsViewModel(id);

            return View("Details", model);
        }
    }
}