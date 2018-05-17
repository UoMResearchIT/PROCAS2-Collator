﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PROCAS2.Models.ServiceBusMessages;
using PROCAS2.Services.App;
using PROCAS2.Resources;

namespace PROCAS2.Services.Utility
{
    public class VolparaService:IVolparaService
    {

        private IWebJobLogger _logger;
        private IWebJobParticipantService _participantService;
        private IScreeningService _screeningService;
        private IAuditService _auditService;

        public VolparaService(IWebJobLogger logger,
                                IWebJobParticipantService participantService,
                                IScreeningService screeningService,
                                IAuditService auditService)
        {
            _logger = logger;
            _participantService = participantService;
            _screeningService = screeningService;
            _auditService = auditService;
        }

        /// <summary>
        /// Process the Volpara screening message
        /// </summary>
        /// <param name="message">message</param>
        /// <returns>List of errors</returns>
        public List<string> ProcessScreeningMessage(string message)
        {
            List<string> retMessages = new List<string>();
            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.Volpara_Screening, WebJobLogLevel.Info, "*** PROCESS SCREENING *** "));

            
            JObject o = JObject.Parse(message);

            IEnumerable<JToken> allImages = o.SelectToken("$.ImageResults");

           
            string patientId = null;

            if (allImages != null)
            {
                // Cycle round every image found (usually 4 but not necessarily)
                foreach (JObject image in allImages)
                {
                    JToken sourceImage = image.SelectToken("$.SourceImage");

                    if (sourceImage != null)
                    {
                        // Get the patient ID
                        JToken thisPatientIdToken = sourceImage.SelectToken("$.Hashes[:1].Value");
                        if (thisPatientIdToken != null)
                        {
                            string thisPatientId = thisPatientIdToken.ToObject<string>();
                            if (!String.IsNullOrEmpty(thisPatientId))
                            {
                                patientId = thisPatientId; // should be the same patientID for all images in the message, but may only appear for first 1!
                            }
                        }

                        // Patient has to exist!
                        if (_participantService.DoesHashedNHSNumberExist(patientId) == false)
                        {
                            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.Volpara_Screening, WebJobLogLevel.Warning, String.Format(VolparaResources.PATIENT_NOT_EXISTS, patientId), messageBody: message));
                            return retMessages;
                        }

                        // Get the image file name
                        JToken imageFilenameToken = sourceImage.SelectToken("$.DicomImageFilePath");
                        if (imageFilenameToken != null)
                        {
                            string imageFileName = imageFilenameToken.ToObject<string>();
                            if (!String.IsNullOrEmpty(imageFileName))
                            {
                   
                            }
                            else
                            {
                                // can't find the property with the file name!
                                retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.Volpara_Screening, WebJobLogLevel.Warning, VolparaResources.FILENAME_NOT_EXISTS, messageBody: message));
                                return retMessages;
                            }
                        }
                        else
                        {
                            // can't find the property with the file name!
                            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.Volpara_Screening, WebJobLogLevel.Warning, VolparaResources.FILENAME_NOT_EXISTS, messageBody: message));
                            return retMessages;
                        }
                    }
                    else
                    {
                        // cant find the section with the image name and patient ID!
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.Volpara_Screening, WebJobLogLevel.Warning, VolparaResources.SOURCEIMAGE_NOT_EXISTS, messageBody: message));
                        return retMessages;
                    }

                    // Get the main screening data section
                    JToken xlsData = image.SelectToken("$.XlsData");
                    if (xlsData != null)
                    {
                        ScreeningXlsMessage xlsMessage = xlsData.ToObject<ScreeningXlsMessage>();

                        StripOutMetaDataInFields(ref xlsMessage);

                        if (_screeningService.CreateScreeningRecord(patientId, xlsMessage) == false)
                        {
                            // can't find the section with all the screening information!
                            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.Volpara_Screening, WebJobLogLevel.Warning, VolparaResources.CANNOT_CREATE_RECORD, messageBody: message));
                            return retMessages;
                        }

                        
                    }
                    else
                    {
                        // can't find the section with all the screening information!
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.Volpara_Screening, WebJobLogLevel.Warning, VolparaResources.XLSDATA_NOT_EXISTS, messageBody: message));
                        return retMessages;
                    }
                }
            }

            return retMessages;
        }


        /// <summary>
        /// Remove the metadata in square brackets. 
        /// </summary>
        /// <param name="xlsMessage"></param>
        private void StripOutMetaDataInFields(ref ScreeningXlsMessage xlsMessage)
        {
            foreach (PropertyInfo pi in typeof(ScreeningXlsMessage).GetProperties())
            {
                if (pi.GetValue(xlsMessage) != null)
                {
                    string val = pi.GetValue(xlsMessage).ToString();
                    int start = val.IndexOf('[');

                    if (start >= 0)
                    {
                        string strippedVal = val.Remove(start).Trim();
                        pi.SetValue(xlsMessage, strippedVal);
                    }
                }
            }
        }
    }
}
