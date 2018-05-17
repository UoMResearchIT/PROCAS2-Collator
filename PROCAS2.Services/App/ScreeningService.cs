﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using PROCAS2.Models.ServiceBusMessages;
using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Resources;

namespace PROCAS2.Services.App
{
    public class ScreeningService : IScreeningService
    {

        IGenericRepository<Participant> _participantRepo;
        IUnitOfWork _unitOfWork;
        IGenericRepository<ScreeningRecordV1_5_4> _screeningRepo;
        IWebJobParticipantService _participantService;

        public ScreeningService(IGenericRepository<Participant> participantRepo,
                                IUnitOfWork unitOfWork,
                                IGenericRepository<ScreeningRecordV1_5_4> screeningRepo,
                                IWebJobParticipantService participantService)
        {
            _participantRepo = participantRepo;
            _unitOfWork = unitOfWork;
            _screeningRepo = screeningRepo;
            _participantService = participantService;
        }

        /// <summary>
        /// Create the screening record from the service bus message
        /// </summary>
        /// <param name="hashedPatientId">Hashed NHS number</param>
        /// <param name="xlsMessage">The message</param>
        /// <returns>true if created, else false</returns>
        public bool CreateScreeningRecord(string hashedPatientId, ScreeningXlsMessage xlsMessage)
        {
            try
            {
                ScreeningRecordV1_5_4 record = HydrateScreeningRecordFromViewModel(xlsMessage);

                record.Participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hashedPatientId).FirstOrDefault();
                record.DataDate = DateTime.Now;
                _screeningRepo.Insert(record);
                _unitOfWork.Save();


                // Created the screening record so record it in the audit trail
                _participantService.AddEvent(record.Participant, _participantService.GetSystemUser(EventResources.VOLPARA_AUTO_USER), DateTime.Now, EventResources.EVENT_VOLPARA, EventResources.EVENT_VOLPARA_STR);

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copy the values from the service bus model to the data entity
        /// </summary>
        /// <param name="xlsMessage">service bus model</param>
        /// <returns>data entity</returns>
        private ScreeningRecordV1_5_4 HydrateScreeningRecordFromViewModel(ScreeningXlsMessage xlsMessage)
        {
            ScreeningRecordV1_5_4 record = new ScreeningRecordV1_5_4();


            foreach (PropertyInfo pi in typeof(ScreeningXlsMessage).GetProperties())
            {
                object modelValue = pi.GetValue(xlsMessage); // Get the value from the model
                if (modelValue != null)
                {
                    PropertyInfo recProp = typeof(ScreeningRecordV1_5_4).GetProperty(pi.Name);
                    if (recProp != null)
                    {
                        recProp.SetValue(record, modelValue.ToString());
                    }
                }
            }


            return record;
        }

       
    }
}
