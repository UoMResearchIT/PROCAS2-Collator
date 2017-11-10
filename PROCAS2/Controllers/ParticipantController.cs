using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;

namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class ParticipantController : Controller
    {

        private IGenericRepository<Participant> _participantRepo;

        public ParticipantController(IGenericRepository<Participant> participantRepo)
        {
            _participantRepo = participantRepo;
        }


        // GET: Participant
        public ActionResult Index()
        {
            ParticipantListViewModel model = new ParticipantListViewModel();

            model.Participants = _participantRepo.GetAll().Where(x => x.Consented == true).ToList();

            return View("Index", model);
        }

        // GET: Participant/Details/5
        public ActionResult Details(int id)
        {
            return View();
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
