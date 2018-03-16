using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Services.Utility;
using PROCAS2.Resources;

namespace PROCAS2.Services.App
{
    public class AuditService:IAuditService
    {
        private IGenericRepository<Participant> _participantRepo;
        private IGenericRepository<ParticipantEvent> _eventRepo;
        private IGenericRepository<EventType> _eventTypeRepo;
        private IUnitOfWork _unitOfWork;
        private IPROCAS2UserManager _userManager;

        public AuditService(IUnitOfWork unitOfWork,
                                IGenericRepository<Participant> participantRepo,
                                IGenericRepository<ParticipantEvent> eventRepo,
                                IPROCAS2UserManager userManager,
                                IGenericRepository<EventType> eventTypeRepo)
        {
            _unitOfWork = unitOfWork;
            _participantRepo = participantRepo;
            _eventRepo = eventRepo;
            _userManager = userManager;
            _eventTypeRepo = eventTypeRepo;
        }

        /// <summary>
        /// Create a audit trail event if the value is changed. For boolean properties
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="participant">Participant object</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns></returns>
        public bool ChangeEventBool(Participant participant, string propertyName, bool oldValue, bool newValue, string reason)
        {
            if (oldValue != newValue)
            {
                AddEvent(participant, _userManager.GetCurrentUser(), DateTime.Now, EventResources.EVENT_PROPERTY_UPDATED, String.Format(EventResources.EVENT_PROPERTY_UPDATED_STR, propertyName, oldValue.ToString(), newValue.ToString()), reason);

            }

            return newValue;
        }

        /// <summary>
        /// Create a audit trail event if the value is changed. For string properties
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="participant">Participant object</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns></returns>
        public string ChangeEventString(Participant participant, string propertyName, string oldValue, string newValue, string reason)
        {
            if (oldValue != newValue)
            {
                AddEvent(participant, _userManager.GetCurrentUser(), DateTime.Now, EventResources.EVENT_PROPERTY_UPDATED, String.Format(EventResources.EVENT_PROPERTY_UPDATED_STR, propertyName, oldValue, newValue), reason);

            }

            return newValue;
        }

        /// <summary>
        /// Create a audit trail event if the value is changed. For date properties
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="participant">Participant object</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns></returns>
        public DateTime ChangeEventDate(Participant participant, string propertyName, DateTime oldValue, DateTime newValue, string reason)
        {
            if (oldValue != newValue)
            {
                AddEvent(participant, _userManager.GetCurrentUser(), DateTime.Now, EventResources.EVENT_PROPERTY_UPDATED, String.Format(EventResources.EVENT_PROPERTY_UPDATED_STR, propertyName, oldValue.ToString("dd/MM/yyyy"), newValue.ToString("dd/MM/yyyy")), reason);

               
            }

            return newValue;
        }

        /// <summary>
        /// Create a audit trail event if the value is changed. For integer properties
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="participant">Participant object</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns></returns>
        public int? ChangeEventInt(Participant participant, string propertyName, int? oldValue, int? newValue, string reason)
        {
            if (oldValue != newValue)
            {
                AddEvent(participant, _userManager.GetCurrentUser(), DateTime.Now, EventResources.EVENT_PROPERTY_UPDATED, String.Format(EventResources.EVENT_PROPERTY_UPDATED_STR, propertyName, oldValue.HasValue ? oldValue.ToString() : "NULL", newValue.HasValue ? newValue.ToString() : "NULL"), reason);

               
            }

            return newValue;
        }

        /// <summary>
        /// Add an event to the passed participant, with the passed information
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
    }
}
