using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Resources;

namespace PROCAS2.Services.App
{
    /// <summary>
    /// Factored out the methods from ParticipantService that webjobs need to function - i.e. removing all context and identity functionality!
    /// </summary>
    public class WebJobParticipantService:IWebJobParticipantService
    {

        private IGenericRepository<Participant> _participantRepo;
        private IGenericRepository<ParticipantEvent> _eventRepo;
        private IGenericRepository<EventType> _eventTypeRepo;
        private IUnitOfWork _unitOfWork;
        private IGenericRepository<AppUser> _appUserRepo;
        private IGenericRepository<RiskLetter> _riskLetterRepo;
       


        public WebJobParticipantService(IGenericRepository<Participant> participantRepo,
                                        IGenericRepository<ParticipantEvent> eventRepo,
                                        IGenericRepository<EventType> eventTypeRepo,
                                        IUnitOfWork unitOfWork,
                                        IGenericRepository<AppUser> appUserRepo,
                                        IGenericRepository<RiskLetter> riskLetterRepo
                                       )
        {
            _participantRepo = participantRepo;
            _eventRepo = eventRepo;
            _eventTypeRepo = eventTypeRepo;
            _unitOfWork = unitOfWork;
            _appUserRepo = appUserRepo;
            _riskLetterRepo = riskLetterRepo;
            
        }

        /// <summary>
        /// Return the AppUser for the requested system user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>AppUser object</returns>
        public AppUser GetSystemUser(string userId)
        {
            AppUser appUser = _appUserRepo.GetAll().Where(x => x.UserCode == userId && x.SystemUser == true && x.Active == true).FirstOrDefault();

            return appUser;
        }

        /// <summary>
        /// Does the passed hash match a participant in the database?
        /// </summary>
        /// <param name="hash">Hashed NHS Number</param>
        /// <returns>true if exists, else false</returns>
        public bool DoesHashedNHSNumberExist(string hash)
        {
            Participant participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hash).FirstOrDefault();
            if (participant == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Set the patient's consented flag to true
        /// </summary>
        /// <param name="hashedNHSNumber">Hashed NHS Number</param>
        /// <returns>true if set OK, else false</returns>
        public bool SetConsentFlag(string hashedNHSNumber)
        {
            try
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hashedNHSNumber).FirstOrDefault();
                if (participant != null)
                {
                    DateTime consentedDate = DateTime.Now;
                    participant.Consented = true;
                    participant.DateConsented = consentedDate;
                    _participantRepo.Update(participant);
                    _unitOfWork.Save();

                   AddEvent(participant, GetSystemUser(EventResources.CRA_AUTO_USER), consentedDate, EventResources.EVENT_CONSENT, EventResources.EVENT_CONSENT_STR);


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

        /// <summary>
        /// Create the patient's risk letter based on the passed content
        /// </summary>
        /// <param name="hashedNHSNumber">Hashed NHS Number</param>
        /// <param name="letterParts">List of paragraphs fofr the letter</param>
        /// <param name="letterGPParts">List of paragraphs for GP letter</param>
        /// <returns>true if successfully created, else false</returns>
        public bool CreateRiskLetter(string hashedNHSNumber, string riskScore, string riskCategory, string geneticTesting, List<string> letterParts, List<string> letterGPParts)
        {
            try
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hashedNHSNumber).FirstOrDefault();
                if (participant != null)
                {

                    RiskLetter letter = new RiskLetter();
                    letter.DateReceived = DateTime.Now;
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < letterParts.Count; i++)
                    {
                        builder.AppendLine("<p>" + letterParts[i] + "</p><p>&nbsp;</p>");
                    }

                    StringBuilder builderGP = new StringBuilder();
                    for (int i = 0; i < letterGPParts.Count; i++)
                    {
                        builderGP.AppendLine("<p>" + letterGPParts[i] + "</p><p>&nbsp;</p>");
                    }

                    letter.RiskLetterContent = builder.ToString();
                    letter.GPLetterContent = builderGP.ToString();
                    letter.RiskCategory = riskCategory;
                    letter.RiskScore = Convert.ToDouble(riskScore);
                    letter.GeneticTestingRecommendation = geneticTesting;
                    letter.Participant = participant;
                    _riskLetterRepo.Insert(letter);
                    _unitOfWork.Save();

                    AddEvent(participant, GetSystemUser(EventResources.CRA_AUTO_USER), DateTime.Now, EventResources.EVENT_RISKLETTER, EventResources.EVENT_RISKLETTER_STR);
 
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


        /// <summary>
        /// Return the study number of the passed NHS hash
        /// </summary>
        /// <param name="hashedNHSNumber">hashed NHS number</param>
        /// <returns>study number</returns>
        public string GetStudyNumber(string hashedNHSNumber)
        {
            Participant participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hashedNHSNumber).FirstOrDefault();
            if (participant != null)
            {
                return participant.StudyNumber.ToString().PadLeft(5, '0');
            }
            else
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Add an event to the passed participant, with the passed information
        /// NB: Copied from auditService - to prevent some Unity resolving issues for webjob
        /// </summary>
        /// <param name="participant">Participant object</param>
        /// <param name="userName">user making the change</param>
        /// <param name="eventDate">date and time of event</param>
        /// <param name="eventCode">event code</param>
        /// <param name="eventNotes">event text</param>
        /// <returns>true if successful, else false</returns>
        public bool AddEvent(Participant participant, AppUser user, DateTime eventDate, string eventCode, string eventNotes, string reason = null)
        {
            try
            {
                ParticipantEvent pEvent = new ParticipantEvent();
                pEvent.AppUser = user;
                pEvent.EventDate = eventDate;
                pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == eventCode).FirstOrDefault();
                pEvent.Notes = eventNotes;
                pEvent.Reason = reason;
                pEvent.Participant = participant;

                _eventRepo.Insert(pEvent);
                _unitOfWork.Save();


                participant.LastEvent = pEvent;
                _participantRepo.Update(participant);

                _unitOfWork.Save();
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Set the patient's BMI
        /// </summary>
        /// <param name="hashedNHSNumber">Hashed NHS Number</param>
        /// <param name="answerText">text containing the BMI</param>
        /// <returns>true if set OK, else false</returns>
        public bool SetBMI(string hashedNHSNumber, string answerText)
        {
            try
            {

                Participant participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hashedNHSNumber).FirstOrDefault();
                if (participant != null)
                {
                    int bmi = 0;
                    if (Int32.TryParse(answerText, out bmi) == false)
                    {
                        participant.BMI = null;
                    }
                    else
                    {
                        participant.BMI = bmi;
                    }
                    _participantRepo.Update(participant);
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
