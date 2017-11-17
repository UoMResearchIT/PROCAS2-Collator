using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data;
using PROCAS2.Data.Entities;

namespace PROCAS2.Services.App
{
    public class DashboardService:IDashboardService
    {
        private IGenericRepository<Participant> _participantRepo;

        public DashboardService(IGenericRepository<Participant> participantRepo)
        {
            _participantRepo = participantRepo;
        }

        /// <summary>
        /// Return the total number of participants
        /// </summary>
        /// <returns>Count of total number of participants</returns>
        public int GetTotalParticipantCount()
        {
            return _participantRepo.GetAll().Count();
        }

        /// <summary>
        /// Return the total number of consented participants
        /// </summary>
        /// <returns>Count of total number of consented participants</returns>
        public int GetConsentedCount()
        {
            return _participantRepo.GetAll().Count(x => x.Consented == true);
        }


        /// <summary>
        /// Return the number of participants who have consented but who haven't yet had any details added.
        /// </summary>
        /// <returns></returns>
        public int GetConsentedNoDetails()
        {
            return _participantRepo.GetAll().Count(x => x.Consented == true && x.LastName != "");
        }
    }
}
