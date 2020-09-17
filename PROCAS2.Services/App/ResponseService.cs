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
        private IGenericRepository<FamilyHistoryItem> _familyRepo;
        private IGenericRepository<FamilyGeneticTestingItem> _familyGeneticRepo;
        private IUnitOfWork _unitOfWork;

        public ResponseService(IGenericRepository<QuestionnaireResponse> responseRepo,
                                IGenericRepository<Participant> participantRepo,
                                IGenericRepository<Question> questionRepo,
                                IGenericRepository<QuestionnaireResponseItem> responseItemRepo,
                                IGenericRepository<FamilyHistoryItem> familyRepo,
                                IGenericRepository<FamilyGeneticTestingItem> familyGeneticRepo,
                                IUnitOfWork unitOfWork)
        {
            _responseRepo = responseRepo;
            _participantRepo = participantRepo;
            _responseItemRepo = responseItemRepo;
            _questionRepo = questionRepo;
            _familyRepo = familyRepo;
            _unitOfWork = unitOfWork;
            _familyGeneticRepo = familyGeneticRepo;
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
                response.DateReceived = DateTime.Now;
                string formatString = "yyyyMMddHHmmss";
                if (String.IsNullOrEmpty(dateStarted) == false)
                {
                    response.QuestionnaireStart = DateTime.ParseExact(dateStarted, formatString, CultureInfo.InvariantCulture);
                }

                if (String.IsNullOrEmpty(dateFinished) == false)
                {
                    response.QuestionnaireEnd = DateTime.ParseExact(dateFinished, formatString, CultureInfo.InvariantCulture);
                }

                response.Participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == patientId && x.Deleted == false).FirstOrDefault();

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
                // Ignore some responses
                if (OnIgnoreList(questionCode) == true)
                {
                    return true;
                }

                QuestionnaireResponseItem item = new QuestionnaireResponseItem();
                item.ResponseText = answerText;
                item.Question = _questionRepo.GetAll().Where(x => x.Code.ToLower() == questionCode.ToLower()).FirstOrDefault();
                if (item.Question == null)
                {
                    item.Question = _questionRepo.GetAll().Where(x => x.Code == "Wibble").FirstOrDefault();
                    item.ResponseText = "(" + questionCode + ") " + item.ResponseText;
                }
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

        /// <summary>
        /// Check to see if the questioncode is on the ignore list
        /// </summary>
        /// <param name="questionCode">true if it is on the list, else false</param>
        private bool OnIgnoreList(string questionCode)
        {
            // The ignore list. Most of these are either recorded in the family history or family genetic testing record. Those that aren't 
            // are metadata or superfluous (duplicates)
            List<string> ignoreList = new List<string>()
            {
                "consentYesNo", "apptID",
                "BRCATestingResult", "CreateBilateral",
                "CurrentBilateral", "HadGeneticTesting",
                "HalfSibRelativeID", "RelativeCancer",
                "RelativeTwin", "WeightPreferredUnits",
                "HeightPreferredUnits", "ProstateCancer",
                "BreastCancer", "bothOvariesRemoved",
                "heightFeetInches",
                "emailAddress", "ColonOrRectalCancer",
                "BMI", "MaternalMaternal",
                "PaternalPaternal", "surveyEnd",
                "Smoking", "UterineCancer",
                "OvarianCancer", "PancreaticCancer",
                "Niece", "CancerChoiceScreen",
                "Consent", "HowMany",
                "PatientName", "surveyStart",
                "WeightAt20PreferredUnits", "riskFactorsConfirmed",
                "hormoneCombined", "hormoneUseYears"
            };

            // Ignore the line if there is no question code (happened in testing!)
            if (String.IsNullOrEmpty(questionCode))
            {
                return true;
            }

            // There is a question called 'Other'. We don't seem to need it...
            if (questionCode == "Other" || questionCode == "Unknown")
            {
                return true;
            }

            // Check each item in the ignore list.
            foreach (string item in ignoreList)
            {
                if (questionCode.StartsWith(item))    
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Create a family history item in the database
        /// </summary>
        /// <param name="response">The questionnaire response this is associated with</param>
        /// <param name="familyHistoryItem">The family history item</param>
        /// <returns>true if created successfully, else false</returns>
        public bool CreateFamilyHistoryItem(QuestionnaireResponse response, FamilyHistoryItem familyHistoryItem)
        {
            try
            {
                if (familyHistoryItem != null && response != null)
                {
                    familyHistoryItem.QuestionnaireResponse = response;

                    _familyRepo.Insert(familyHistoryItem);
                    _unitOfWork.Save();
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create a family genetic testing item in the database
        /// </summary>
        /// <param name="response">The questionnaire response this is associated with</param>
        /// <param name="familyHistoryItem">The family genetic testing item</param>
        /// <returns>true if created successfully, else false</returns>
        public bool CreateFamilyGeneticTestingItem(QuestionnaireResponse response, FamilyGeneticTestingItem familyGeneticTestingItem)
        {
            try
            {
                if (familyGeneticTestingItem != null && response != null)
                {
                    familyGeneticTestingItem.QuestionnaireResponse = response;

                    _familyGeneticRepo.Insert(familyGeneticTestingItem);
                    _unitOfWork.Save();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
