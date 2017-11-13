using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;
using PROCAS2.Services.App;

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
                if (_participantService.UploadNewParticipants(model, out outModel, out hashFile) == false)
                {
                    // if there is a problem uploading the participants say so.
                    return View("UploadResults", outModel);

                }
                else
                {
                    // If there is no problem then return a spreadsheet with the hash codes.

                }
            }

            return View("UploadNew", model);
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
    }
}
