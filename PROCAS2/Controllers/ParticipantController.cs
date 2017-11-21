using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;
using PROCAS2.Services.App;
using PROCAS2.CustomActionResults;

namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class ParticipantController : Controller
    {

        private IGenericRepository<Participant> _participantRepo;
        private IParticipantService _participantService;

        public ParticipantController(IGenericRepository<Participant> participantRepo,
                                    IParticipantService participantService)
        {
            _participantRepo = participantRepo;
            _participantService = participantService;
        }


        // GET: Participant
        public ActionResult Index()
        {
            ParticipantListViewModel model = new ParticipantListViewModel();

            model.Participants = _participantRepo.GetAll().Where(x => x.Consented == true).ToList();

            return View("Index", model);
        }

        // GET: Participant/Upload
        public ActionResult UploadNew()
        {
            UploadNewParticipantsViewModel model = new UploadNewParticipantsViewModel();
            return View("UploadNew", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadNew(UploadNewParticipantsViewModel model)
        {
            if (ModelState.IsValid)
            {
                MemoryStream hashFile;
                UploadResultsViewModel outModel;

                Response.Clear();

                var requestToken = Request.Cookies["fileDownloadToken"];
                if (requestToken != null && long.Parse(requestToken.Value) > 0)
                {
                    var responseTokenValue = long.Parse(requestToken.Value) * (-1);
                    Response.Cookies["fileDownloadToken"].Value = responseTokenValue.ToString();
                    Response.Cookies["fileDownloadToken"].Path = "/";
                }

                Response.Buffer = true;

                if (_participantService.UploadNewParticipants(model, out outModel, out hashFile) == false)
                {
                    // if there is a problem uploading the participants say so.
                    return View("UploadResults", outModel);

                }
                else
                {
                    // If there is no problem then return a spreadsheet with the hash codes.
                    string headerValue = string.Concat(1, ";Url=", PrependSchemeAndAuthority("Participant/UploadNew"));
                    HttpContext.Response.AppendHeader("Refresh", headerValue);

                    

                    return new CSVResult(hashFile, "Hashes.csv");
                }
            }

            return View("UploadNew", model);
        }

        // GET: Participant/Upload
        public ActionResult UploadUpdate()
        {
            UploadUpdateParticipantsViewModel model = new UploadUpdateParticipantsViewModel();
            return View("UploadUpdate", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadUpdate(UploadUpdateParticipantsViewModel model)
        {
            if (ModelState.IsValid)
            {
               
                UploadResultsViewModel outModel;

                Response.Clear();

                var requestToken = Request.Cookies["fileDownloadToken"];
                if (requestToken != null && long.Parse(requestToken.Value) > 0)
                {
                    var responseTokenValue = long.Parse(requestToken.Value) * (-1);
                    Response.Cookies["fileDownloadToken"].Value = responseTokenValue.ToString();
                    Response.Cookies["fileDownloadToken"].Path = "/";
                }

                Response.Buffer = true;

                _participantService.UploadUpdateParticipants(model, out outModel);
                
                return View("UploadResults", outModel);

               
               
            }

            return View("UploadUpdate", model);
        }

        // GET: Participant/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Participant/Create
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

        // GET: Participant/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Participant/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Participant/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Participant/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
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
