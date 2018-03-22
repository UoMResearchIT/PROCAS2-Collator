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
        private IGenericRepository<ScreeningSite> _screeningSiteRepo;
        private IParticipantService _participantService;

        public ParticipantController(IGenericRepository<Participant> participantRepo,
                                    IParticipantService participantService,
                                    IGenericRepository<ScreeningSite> screeningSiteRepo)
        {
            _participantRepo = participantRepo;
            _participantService = participantService;
            _screeningSiteRepo = screeningSiteRepo;
        }


        // GET: Participant
        public ActionResult Index()
        {
            ParticipantListViewModel model = new ParticipantListViewModel();

            model.Participants = _participantRepo.GetAll().Where(x => x.Consented == true && x.FirstName != null).ToList();

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

                    

                    return new TextResult(hashFile, "Hashes.txt");
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


        // GET: Participant/Upload
        public ActionResult UploadAskRisk()
        {
            UploadAskRiskViewModel model = new UploadAskRiskViewModel();
            return View("UploadAskRisk", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadAskRisk(UploadAskRiskViewModel model)
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

                _participantService.UploadAskRisk(model, out outModel);

                return View("UploadResults", outModel);

            }

            return View("UploadAskRisk", model);
        }


        // GET: Participant/View/Id
        public ActionResult Details(string participantId)
        {
            ParticipantDetailsViewModel model = new ParticipantDetailsViewModel();

            model.Participant = _participantRepo.GetAll().Where(x => x.NHSNumber == participantId).FirstOrDefault();
            if (model.Participant != null)
            {
                return View("Details", model);
            }

            // Can't find the participant so just return to the index
            return RedirectToAction("Index");
        }


        // GET: Participant/Edit/NHS123
        public ActionResult Edit(string participantId)
        {
            ParticipantEditViewModel model = new ParticipantEditViewModel();

            model.Participant = _participantRepo.GetAll().Where(x => x.NHSNumber == participantId).FirstOrDefault();
            if (model.Participant != null)
            {
                model.ScreeningSites = _screeningSiteRepo.GetAll().OrderBy(x=>x.Name).ToList();
                model.ScreeningSite = model.Participant.ScreeningSite.Code;
                model.Reason = "";
                return View("Edit", model);
            }

            // Can't find the participant so just return to the index
            return RedirectToAction("Index");
        }

        // POST: Participant/Edit/NHS123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ParticipantEditViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<string> errors;
                    errors = _participantService.UpdateParticipantFromUI(model);

                    if (errors.Count == 0) // no errors from updating
                    {
                        return RedirectToAction("Index");
                    }
                    else // some errors from updating
                    {
                        foreach (string error in errors)
                        {
                            ModelState.AddModelError("", error);
                        }

                        model.ScreeningSites = _screeningSiteRepo.GetAll().OrderBy(x => x.Name).ToList();
                        return View("Edit", model);
                    }
                }

                model.ScreeningSites = _screeningSiteRepo.GetAll().OrderBy(x => x.Name).ToList();
                return View("Edit", model);
                
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        // POST: Participant/Delete/NHS123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            bool del = false;
            if (String.IsNullOrEmpty(id) == false)
            {
                if (_participantService.DeleteParticipant(id) == true)
                {
                    del = true;
                }
            }

            return Json(new { deleted = del });
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
