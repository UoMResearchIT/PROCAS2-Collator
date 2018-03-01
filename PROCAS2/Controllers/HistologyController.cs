using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Models.ViewModels;
using PROCAS2.Data;
using PROCAS2.Data.Entities;

namespace PROCAS2.Controllers
{

#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    public class HistologyController : Controller
    {

        private IGenericRepository<Participant> _participantRepo;

        public HistologyController(IGenericRepository<Participant> participantRepo)
        {
            _participantRepo = participantRepo;
        }


        // GET: Histology
        public ActionResult Index()
        {
            HistologyListViewModel model = new HistologyListViewModel();

            model.Participants = _participantRepo.GetAll().Where(x => x.Consented == true && x.FirstName != null).ToList();

            return View("Index", model);
        }
    }
}