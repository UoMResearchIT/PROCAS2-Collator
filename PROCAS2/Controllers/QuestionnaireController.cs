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
    public class QuestionnaireController : Controller
    {
        private IGenericRepository<QuestionnaireResponse> _responseRepo;

        public QuestionnaireController(IGenericRepository<QuestionnaireResponse> responseRepo)
        {
            _responseRepo = responseRepo;
        }


        // GET: Questionnaire
        public ActionResult Details(string responseId)
        {
            QuestionnaireDetailsViewModel model = new QuestionnaireDetailsViewModel();

            int respId;

            if (Int32.TryParse(responseId, out respId) == true)
            {
                QuestionnaireResponse response = _responseRepo.GetAll().Where(x => x.Id == respId).FirstOrDefault();
                if (response != null)
                {
                    model.NHSNumber = response.Participant.NHSNumber;
                    model.QuestionnaireEnd = response.QuestionnaireEnd;
                    model.QuestionnaireStart = response.QuestionnaireStart;
                    model.ResponseItems = response.QuestionnaireResponseItems.OrderBy(x =>x.Question.Code).ToList();
                    model.HistoryItems = response.FamilyHistoryItems.ToList();
                }
            }

            

            return View("Details", model);
        }
    }
}