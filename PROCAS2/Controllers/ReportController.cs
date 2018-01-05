using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Models.ViewModels.Reports;

namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class ReportController : Controller
    {
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

            return View("PatientDetails", model);
        }
    }
}