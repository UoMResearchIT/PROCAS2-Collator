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


        public ScreeningController(IGenericRepository<ScreeningRecordV1_5_2> screeningV1_5_2Repo)
        {
            _screeningV1_5_2Repo = screeningV1_5_2Repo;
        }

        // GET: Screening
        public ActionResult ViewScreenV1_5_2(string screenId)
        {
            ScreenV1_5_2DetailsViewModel model = new ScreenV1_5_2DetailsViewModel();

            int scrId;

            if (Int32.TryParse(screenId, out scrId) == false)
            {
                return null;
            }

            model.ScreeningRecord = _screeningV1_5_2Repo.GetAll().Where(x => x.Id == scrId).FirstOrDefault();

            return View("ViewScreenV1_5_2", model);
        }
    }
}