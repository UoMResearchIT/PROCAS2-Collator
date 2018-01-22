using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using PROCAS2.Data;
using PROCAS2.Data.Entities;



namespace PROCAS2.Services.App
{
    public class ResponseService :IResponseService
    {
        private IGenericRepository<QuestionnaireResponse> _responseRepo;
        private IGenericRepository<Participant> _participantRepo;
        private IGenericRepository<Question> _questionRepo;
        private IGenericRepository<QuestionnaireResponseItem> _responseItemRepo;
        private IUnitOfWork _unitOfWork;

        public ResponseService(IGenericRepository<QuestionnaireResponse> responseRepo,
                                IGenericRepository<Participant> participantRepo,
                                IGenericRepository<Question> questionRepo,
                                IGenericRepository<QuestionnaireResponseItem> responseItemRepo,
                                IUnitOfWork unitOfWork)
        {
            _responseRepo = responseRepo;
            _participantRepo = participantRepo;
            _responseItemRepo = responseItemRepo;
            _questionRepo = questionRepo;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Create and return a response header 
        /// </summary>
        /// <param name="patientId">Hashed patient ID (NHS number)</param>
        /// <param name="dateStarted">Date the Questionnaire was started</param>
        /// <param name="dateFinished">Date the questionnaire was finished</param>
        /// <param name="response">The response header object</param>
        /// <returns>true if the header was created, else false</returns>
        public bool CreateQuestionnaireHeader(string patientId, string dateStarted, string dateFinished, out QuestionnaireResponse response)
        {
            try
            {
                response = new QuestionnaireResponse();
                string formatString = "yyyyMMddHHmmss";
                if (String.IsNullOrEmpty(dateStarted) == false)
                {
                    response.QuestionnaireStart = DateTime.ParseExact(dateStarted, formatString, CultureInfo.InvariantCulture);
                }

                if (String.IsNullOrEmpty(dateFinished) == false)
                {
                    response.QuestionnaireEnd = DateTime.ParseExact(dateFinished, formatString, CultureInfo.InvariantCulture);
                }

                response.Participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == patientId).FirstOrDefault();

                _responseRepo.Insert(response);
                _unitOfWork.Save();
            }
            catch(Exception ex)
            {
                response = null;
                return false;
            }

            return true;

        }

        /// <summary>
        /// Create a questionnaire response item based on the passed information
        /// </summary>
        /// <param name="response">The response header</param>
        /// <param name="questionCode">The question code</param>
        /// <param name="answerText">The answer text</param>
        /// <returns>true if created, else false</returns>
        public bool CreateResponseItem(QuestionnaireResponse response, string questionCode, string answerText)
        {
            try
            {
                QuestionnaireResponseItem item = new QuestionnaireResponseItem();
                item.ResponseText = answerText;
                item.Question = _questionRepo.GetAll().Where(x => x.Code == questionCode).FirstOrDefault();
                item.QuestionnaireResponse = response;

                _responseItemRepo.Insert(item);
                _unitOfWork.Save();
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
