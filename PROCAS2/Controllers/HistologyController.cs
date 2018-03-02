using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Models.ViewModels;
using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Services.App;

namespace PROCAS2.Controllers
{

#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class HistologyController : Controller
    {

        private IGenericRepository<Participant> _participantRepo;
        private IHistologyService _histologyService;

        public HistologyController(IGenericRepository<Participant> participantRepo,
                            IHistologyService histologyService)
        {
            _participantRepo = participantRepo;
            _histologyService = histologyService;
        }


        // GET: Histology
        public ActionResult Index()
        {
            HistologyListViewModel model = new HistologyListViewModel();

            model.Participants = _participantRepo.GetAll().Where(x => x.Consented == true && x.FirstName != null).ToList();

            return View("Index", model);
        }

        [HttpGet]
        public ActionResult Edit(string participantId)
        {
            if (!String.IsNullOrEmpty(participantId))
            {
                HistologyEditViewModel model = new HistologyEditViewModel();

                model = _histologyService.FillEditViewModel(participantId);

                return View("Edit", model);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HistologyEditViewModel model, string btnSaveAndReturn)
        {
            if (ModelState.IsValid)
            {
                int headerId = _histologyService.SaveHeader(model);
                if (headerId > 0)
                {
                    if (String.IsNullOrEmpty(btnSaveAndReturn))
                    {
                        model.HeaderId = headerId;
                        model.DiagnosisSides = _histologyService.GetLookups("SIDE");
                        model.DiagnosisTypes = _histologyService.GetLookups("TYPE");
                        model.fromSave = true;
                        return View("Edit", model);
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    model.DiagnosisSides = _histologyService.GetLookups("SIDE");
                    model.DiagnosisTypes = _histologyService.GetLookups("TYPE");
                    return View("Edit", model);
                }
                
            }
            else
            {
                model.DiagnosisSides = _histologyService.GetLookups("SIDE");
                model.DiagnosisTypes = _histologyService.GetLookups("TYPE");
                return View("Edit", model);
            }
        }
    }
}