using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

using PROCAS2.Models.ViewModels;
using PROCAS2.Services.App;
using PROCAS2.CustomActionResults;

namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class ExportController : Controller
    {

        private IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        // GET: Export
        public ActionResult Export()
        {
            ExportLettersViewModel model = new ExportLettersViewModel();

            model.AllReady = true; // default to true

            return View("Export", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Export(ExportLettersViewModel model)
        {

            if (ModelState.IsValid == true)
            {
                ExportResultsViewModel results = _exportService.GenerateLetters(model);
                string html = _exportService.RenderRazorViewToString(ControllerContext, results, "ExportResults");
                MemoryStream mStream = _exportService.GenerateWordDoc(html);
                return new WordResult(mStream, "Letters");
            }
            else
                return View("Export", model);

        }
    }
}