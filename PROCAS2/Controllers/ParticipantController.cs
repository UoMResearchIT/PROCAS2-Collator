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
using PROCAS2.Services.Utility;
using PROCAS2.CustomActionResults;
using PROCAS2.Resources;

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
        private IGenericRepository<Image> _imageRepo;
        private IParticipantService _participantService;
        private IStorageService _storageService;

        public ParticipantController(IGenericRepository<Participant> participantRepo,
                                    IParticipantService participantService,
                                    IGenericRepository<ScreeningSite> screeningSiteRepo,
                                    IGenericRepository<Image> imageRepo,
                                    IStorageService storageService)
        {
            _participantRepo = participantRepo;
            _participantService = participantService;
            _screeningSiteRepo = screeningSiteRepo;
            _storageService = storageService;
            _imageRepo = imageRepo;
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


        // GET: Participant/Upload
        public ActionResult UploadScreeningOutcomes()
        {
            UploadScreeningOutcomesViewModel model = new UploadScreeningOutcomesViewModel();
            return View("UploadScreeningOutcomes", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadScreeningOutcomes(UploadScreeningOutcomesViewModel model)
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

                _participantService.UploadScreeningOutcomes(model, out outModel);

                return View("UploadResults", outModel);



            }

            return View("UploadScreeningOutcomes", model);
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
                model.ScreeningSite = model.Participant.ScreeningSite.Code;
                model.ChemoPreventionDetailsId = model.Participant.ChemoPreventionDetailsId;
                model.InitialScreeningOutcomeId = model.Participant.InitialScreeningOutcomeId;
                model.FinalAssessmentOutcomeId = model.Participant.FinalAssessmentOutcomeId;
                model.FinalTechnicalOutcomeId = model.Participant.FinalTechnicalOutcomeId;
                model.RiskConsultationTypeId = model.Participant.RiskConsultationTypeId;
                FillEditLookups(ref model);
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

                        FillEditLookups(ref model);
                        return View("Edit", model);
                    }
                }

                FillEditLookups(ref model);
                return View("Edit", model);
                
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Fill all the drop down boxes on the edit screen
        /// </summary>
        /// <param name="model">The edit view model</param>
        private void FillEditLookups(ref ParticipantEditViewModel model)
        {
            model.ScreeningSites = _screeningSiteRepo.GetAll().OrderBy(x => x.Name).ToList();
            
            model.ChemoPreventionDetails = _participantService.GetLookups("CHEMO");
            
            model.InitialScreeningOutcome = _participantService.GetLookups("INITIAL");
            
            model.FinalAssessmentScreeningOutcome = _participantService.GetLookups("RECALL");
            
            model.FinalTechnicalScreeningOutcome = _participantService.GetLookups("TECH");
            
            
            model.RiskConsultationTypes = _participantService.GetLookups("RISKCONS");
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

        [HttpGet]
        public ActionResult DeleteParticipant()
        {
            DeleteParticipantViewModel model = new DeleteParticipantViewModel();

            return View("Delete", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteParticipant(DeleteParticipantViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_participantService.DeleteParticipant(model.NHSNumber) == false)
                {
                    ModelState.AddModelError("NHSNumber", ParticipantResources.CANNOT_DELETE );
                    return View("Delete", model);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ParticipantHistory()
        {
            ParticipantHistoryQueryViewModel model = new ParticipantHistoryQueryViewModel();

            return View("ParticipantHistoryQuery", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ParticipantHistory(ParticipantHistoryQueryViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                ParticipantHistoryDetailsViewModel detailModel = new ParticipantHistoryDetailsViewModel();
                if (_participantService.GetParticipantHistory(model.NHSNumber, ref detailModel) == false)
                {
                    ModelState.AddModelError("NHSNumber", ParticipantResources.CANNOT_FIND_HISTORY);
                    return View("ParticipantHistoryQuery", model);
                }
                else
                {
                    return View("ParticipantHistoryDetails", detailModel);
                }
            }

            return View("ParticipantHistoryQuery", model);
        }


        [HttpGet]
        public ActionResult ConsentForm(int studyNumber)
        {
            
            Participant participant = _participantRepo.GetAll().Where(x => x.StudyNumber == studyNumber).FirstOrDefault();
            if (participant != null)
            {
                if (_storageService.ConsentFormExists(studyNumber) == true)
                {
                    MemoryStream consentStream = _storageService.GetConsentForm(studyNumber);
                    if (consentStream != null)
                    {
                        return new FileStreamResult(consentStream, "application/pdf") { FileDownloadName = "Consent-" + studyNumber.ToString().PadLeft(5, '0') + ".pdf" };
                    }
                    else
                    {
                        return View("NoConsent");
                    }
                }
                else
                {
                    return View("NoConsent");
                }
            }

            return View("NoConsent");
        }

        [HttpGet]
        public ActionResult GetImage(string imageName)
        {

            Image image = _imageRepo.GetAll().Where(x => x.CurrentName == imageName).FirstOrDefault();
            if (image != null)
            {
                if (_storageService.ImageExists(imageName) == true)
                {
                    MemoryStream imageStream = _storageService.GetImage(imageName);
                    if (imageStream != null)
                    {
                        return new FileStreamResult(imageStream, "application/dicom") { FileDownloadName = imageName };
                    }
                    else
                    {
                        return View("NoImage");
                    }
                }
                else
                {
                    return View("NoImage");
                }
            }

            return View("NoImage");
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
