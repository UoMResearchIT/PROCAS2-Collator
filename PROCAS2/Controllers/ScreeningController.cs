﻿using System;
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
    public class ScreeningController : Controller
    {
        private IGenericRepository<ScreeningRecordV1_5_4> _screeningV1_5_4Repo;
        private IGenericRepository<Participant> _participantRepo;
        private IGenericRepository<VolparaDensity> _densityRepo;


        public ScreeningController(IGenericRepository<ScreeningRecordV1_5_4> screeningV1_5_4Repo,
                                    IGenericRepository<Participant> participantRepo,
                                    IGenericRepository<VolparaDensity> densityRepo)
        {
            _screeningV1_5_4Repo = screeningV1_5_4Repo;
            _participantRepo = participantRepo;
            _densityRepo = densityRepo;
        }

        // GET: Screening
        public ActionResult ViewScreenV1_5_4(string id)
        {
            ScreenV1_5_4DetailsViewModel model = new ScreenV1_5_4DetailsViewModel();

            model.NHSNumber = id;

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == id).FirstOrDefault();
            if (participant != null)
            {

                model.ScreeningRecords = _screeningV1_5_4Repo.GetAll().Where(x => x.Participant.NHSNumber == id).OrderByDescending(x => x.DataDate).ThenBy(x => x.MammoView).ToList();
            }

            return View("ViewScreenV1_5_4", model);
        }

        // GET: Screening
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
    }
}