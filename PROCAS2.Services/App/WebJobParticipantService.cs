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
                                        IGenericRepository<RiskLetter> riskLetterRepo)
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
                    participant.Consented = true;
                    _participantRepo.Update(participant);
                    _unitOfWork.Save();

                    ParticipantEvent pEvent = new ParticipantEvent();
                    pEvent.Participant = participant;
                    pEvent.EventDate = DateTime.Now;
                    pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_CONSENT).FirstOrDefault();
                    pEvent.Notes = EventResources.EVENT_CONSENT_STR;
                    pEvent.AppUser = GetSystemUser(EventResources.CRA_AUTO_USER);
                    _eventRepo.Insert(pEvent);
                    _unitOfWork.Save();

                    participant.LastEvent = pEvent;
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

        /// <summary>
        /// Create the patient's risk letter based on the passed content
        /// </summary>
        /// <param name="hashedNHSNumber">Hashed NHS Number</param>
        /// <param name="letterParts">List of paragraphs fofr the letter</param>
        /// <returns>true if successfully created, else false</returns>
        public bool CreateRiskLetter(string hashedNHSNumber, string riskScore, string riskCategory, List<string> letterParts)
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
                    letter.RiskLetterContent = builder.ToString();
                    letter.RiskCategory = riskCategory;
                    letter.RiskScore = Convert.ToDouble(riskScore);
                    letter.Participant = participant;
                    _riskLetterRepo.Insert(letter);
                    _unitOfWork.Save();

                    ParticipantEvent pEvent = new ParticipantEvent();
                    pEvent.Participant = participant;
                    pEvent.EventDate = DateTime.Now;
                    pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_RISKLETTER).FirstOrDefault();
                    pEvent.Notes = EventResources.EVENT_RISKLETTER_STR;
                    pEvent.AppUser = GetSystemUser(EventResources.CRA_AUTO_USER);
                    _eventRepo.Insert(pEvent);
                    _unitOfWork.Save();

                    participant.LastEvent = pEvent;
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
