using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;

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
                string errString;

                if (model.AllReady == false)
                {
                    if (_exportService.ValidateNHSNumberForExport(model.NHSNumber, out errString) == false)
                    {
                        // error on validating NHS number
                        ModelState.AddModelError("NHSNumber", errString);
                        return View("Export", model);
                    }
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

                ExportResultsViewModel results = _exportService.GenerateLetters(model);
                string html = _exportService.RenderRazorViewToString(ControllerContext, results, "ExportResults");
                MemoryStream mStream = _exportService.GenerateWordDoc(html, results);

                string headerValue = string.Concat(1, ";Url=", PrependSchemeAndAuthority("Export"));
                HttpContext.Response.AppendHeader("Refresh", headerValue);

                //TODO: set the sent risk flag

                return new WordResult(mStream, "Letters");


            }
            else
                return View("Export", model);

        }

        [HttpGet]
        public ActionResult ViewLetter(string letterId)
        {
            ExportResultsViewModel results = _exportService.GenerateLetters(letterId);

            if (results.Letters.Count == 0)
            {
                return null;
            }

            string html = _exportService.RenderRazorViewToString(ControllerContext, results, "ExportResults");
            MemoryStream mStream = _exportService.GenerateWordDoc(html, results);

        

            return new WordResult(mStream, "Letter");
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