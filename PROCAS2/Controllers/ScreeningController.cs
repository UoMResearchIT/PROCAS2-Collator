using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

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
    public class ScreeningController : Controller
    {
        private IGenericRepository<ScreeningRecordV1_5_4> _screeningV1_5_4Repo;
        private IGenericRepository<Participant> _participantRepo;
        private IGenericRepository<VolparaDensity> _densityRepo;
        private IScreeningService _screeningService;


        public ScreeningController(IGenericRepository<ScreeningRecordV1_5_4> screeningV1_5_4Repo,
                                    IGenericRepository<Participant> participantRepo,
                                    IGenericRepository<VolparaDensity> densityRepo,
                                    IScreeningService screeningService)
        {
            _screeningV1_5_4Repo = screeningV1_5_4Repo;
            _participantRepo = participantRepo;
            _densityRepo = densityRepo;
            _screeningService = screeningService;
        }

        // GET: Screening
        public ActionResult ViewScreenV1_5_4(string id)
        {
            ScreenV1_5_4DetailsViewModel model = new ScreenV1_5_4DetailsViewModel();

            model.NHSNumber = id;

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == id).FirstOrDefault();
            if (participant != null)
            {

                model.ScreeningRecords = _screeningV1_5_4Repo.GetAll().Include(x => x.Image).Where(x => x.Participant.NHSNumber == id).OrderByDescending(x => x.DataDate).ThenBy(x => x.MammoView).ToList();
            }

            return View("ViewScreenV1_5_4", model);
        }

        // GET: Screening
        [HttpGet]
        public ActionResult Density(int id)
        {
            DensityViewModel model = new DensityViewModel();

            if (id != 0)
            {

                VolparaDensity density = _densityRepo.GetAll().Where(x => x.Id == id).FirstOrDefault();
                if (density != null)
                {
                    model.VolparaDensity = density;
                    return View("Density", model);
                }
            }

            return View("Density", model);
        }

        // POST: Screening
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Density(DensityViewModel model)
        {

            if (ModelState.IsValid)
            {
                if (model.VolparaDensity.Id != 0)
                {

                    if(_screeningService.ToggleUsingScoreCard(model.VolparaDensity.Id) == false)
                    { 
                        return View("Density", model);
                    }
                    else
                    {
                        return RedirectToAction("Density", "Screening", new { id = model.VolparaDensity.Id });
                    }
                }
            }

            return View("Density", model);
        }
    }
}