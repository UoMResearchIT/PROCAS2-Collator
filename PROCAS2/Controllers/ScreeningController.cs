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
    public class ScreeningController : Controller
    {
        private IGenericRepository<ScreeningRecordV1_5_2> _screeningV1_5_2Repo;
        private IGenericRepository<Participant> _participantRepo;


        public ScreeningController(IGenericRepository<ScreeningRecordV1_5_2> screeningV1_5_2Repo,
                                    IGenericRepository<Participant> participantRepo)
        {
            _screeningV1_5_2Repo = screeningV1_5_2Repo;
            _participantRepo = participantRepo;
        }

        // GET: Screening
        public ActionResult ViewScreenV1_5_2(string id)
        {
            ScreenV1_5_2DetailsViewModel model = new ScreenV1_5_2DetailsViewModel();

            model.NHSNumber = id;

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == id).FirstOrDefault();
            if (participant != null)
            {

                model.ScreeningRecords = _screeningV1_5_2Repo.GetAll().Where(x => x.Participant.NHSNumber == id).ToList();
            }

            return View("ViewScreenV1_5_2", model);
        }
    }
}