using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Models.TestModels;
using PROCAS2.Data.Entities;
using PROCAS2.Services.App;
using PROCAS2.Services.Utility;
using PROCAS2.Data;

namespace PROCAS2.Controllers
{
    public class Phase1Controller : Controller
    {
        private IParticipantService _participantService;
        private IGenericRepository<Participant> _participantRepo;
        private IServiceBusService _serviceBusService;

        public Phase1Controller(ParticipantService participantService,
                                IGenericRepository<Participant> participantRepo,
                                IServiceBusService serviceBusService)
        {
            _participantService = participantService;
            _participantRepo = participantRepo;
            _serviceBusService = serviceBusService;
        }


        [HttpGet]
        public ActionResult ReceiveConsent()
        {
            ReceiveConsentViewModel model = new ReceiveConsentViewModel();
            model.DateOfConsent = DateTime.Now;
            return View("ReceiveConsent", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReceiveConsent(ReceiveConsentViewModel model)
        {
            if (ModelState.IsValid)
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == model.NHSNumber).FirstOrDefault();
                if (participant != null)
                {
                   
                    if (_participantService.SetConsentFlag(participant.HashedNHSNumber, model.DateOfConsent) == false)
                    {
                        ModelState.AddModelError("NHSNumber", "Error setting consent");
                        return View("ReceiveConsent", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("NHSNumber", "Participant does not exist");
                    return View("ReceiveConsent", model);
                }
            }
            else
            {
                return View("ReceiveConsent", model);
            }


            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public ActionResult SendVolpara()
        {
            SendVolparaViewModel model = new SendVolparaViewModel();
            return View("SendVolpara", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendVolpara(SendVolparaViewModel model)
        {
            if (ModelState.IsValid)
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == model.NHSNumber).FirstOrDefault();
                if (participant != null)
                {
                    string message = @"{ 'HashedPatientId' : '" + participant.HashedNHSNumber + "', 'DateOfConsent':'" + DateTime.Now.ToString("yyyy-MM-dd") + "'}";
                    if (_serviceBusService.PostServiceBusMessage("Volpara-ServiceBusKeyName", "Volpara-ServiceBusKeyValue", "Volpara-ServiceBusBase", message, "VolparaConsentQueue", false) == false)
                    {
                        ModelState.AddModelError("NHSNumber", "Error setting consent");
                        return View("SendVolpara", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("NHSNumber", "Participant does not exist");
                    return View("SendVolpara", model);
                }
            }


            return View("SendVolpara", model);
        }
    }
}