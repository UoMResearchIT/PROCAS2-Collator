using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Services.Utility;

namespace PROCAS2.Services.App
{
    public class DashboardService:IDashboardService
    {
        private IGenericRepository<Participant> _participantRepo;
        private IConfigService _configService;

        public DashboardService(IGenericRepository<Participant> participantRepo,
                                IConfigService configService)
        {
            _participantRepo = participantRepo;
            _configService = configService;
        }

        /// <summary>
        /// Return the total number of participants
        /// </summary>
        /// <returns>Count of total number of participants</returns>
        public int GetTotalParticipantCount()
        {
            DateTime invitedSince = DateTime.Now.AddMonths(-2);
          
            return _participantRepo.GetAll().Count(x => x.Deleted == false && x.DateCreated.HasValue && x.DateCreated > invitedSince);
        }

        /// <summary>
        /// Return the total number of consented participants
        /// </summary>
        /// <returns>Count of total number of consented participants</returns>
        public int GetConsentedCount()
        {
            return _participantRepo.GetAll().Count(x => x.Consented == true && x.Deleted == false);
        }


        /// <summary>
        /// Return the number of participants who have consented but who haven't yet had any details added.
        /// </summary>
        /// <returns></returns>
        public int GetConsentedNoDetails()
        {
            return _participantRepo.GetAll().Count(x => x.Consented == true && x.LastName == null && x.Deleted == false);
        }

        /// <summary>
        /// Return the number of participants who have received Volpara data but risk letter has not yet been asked for
        /// </summary>
        /// <returns>The count</returns>
        public int GetRiskLetterNotAskedFor()
        {
            return _participantRepo.GetAll().Include(a => a.ScreeningRecordV1_5_4s).Count(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == false && x.ScreeningRecordV1_5_4s.Count > 0 && x.Deleted == false);
        }

        /// <summary>
        /// Return the number of participants who have not received Volpara data but have consented
        /// </summary>
        /// <returns>The count</returns>
        public int GetWaitingForVolpara()
        {
            return _participantRepo.GetAll().Include(a => a.ScreeningRecordV1_5_4s).Count(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == false && x.ScreeningRecordV1_5_4s.Count == 0 && x.Deleted == false);
        }

        /// <summary>
        /// Return the number of participants who have not received Volpara data but have consented, and are close to the 6 week deadline
        /// </summary>
        /// <returns>The count</returns>
        public int GetWaitingForVolparaNear6Weeks()
        {
            int noVolparaWarningDays = _configService.GetIntAppSetting("NoVolparaWarningDays") ?? 0;
            return _participantRepo.GetAll().Include(a => a.ScreeningRecordV1_5_4s).Count(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == false && x.ScreeningRecordV1_5_4s.Count == 0 && x.Deleted == false && x.DateFirstAppointment.HasValue &&  DbFunctions.DiffDays(x.DateFirstAppointment.Value, DateTime.Now) >= noVolparaWarningDays );
        }

        /// <summary>
        /// Return the number of participants who have got a risk letter but it has not been sent
        /// </summary>
        /// <returns>The count</returns>
        public int GetLetterNotSent()
        {
            return _participantRepo.GetAll().Include(a => a.RiskLetters).Count(x => x.Consented == true && x.LastName != null &&  x.SentRisk == false && x.RiskLetters.Count > 0 && x.Deleted == false);
        }


        /// <summary>
        /// Return the number of participants waiting for a letter to be received from CRA
        /// </summary>
        /// <returns>The count</returns>
        public int GetWaitingForLetter()
        {
            //List<Participant> parts = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.SentRisk == false && x.RiskLetters.Count == 0).ToList();
            return _participantRepo.GetAll().Include(a => a.RiskLetters).Count(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == true && x.SentRisk == false && x.RiskLetters.Count == 0 && x.Deleted == false);
        }
    }
}
