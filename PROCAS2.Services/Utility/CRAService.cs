using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;


using NHapi.Base;
using NHapi.Base.Parser;
using NHapi.Base.Util;
using NHapi.Base.SourceGeneration;
using NHapi.Base.Model;
using NHapi.Model.V23.Datatype;
using NHapi.Model.V23.Group;
using NHapi.Model.V23.Segment;
using NHapi.Model.V23.Message;

using Newtonsoft.Json;

using PROCAS2.Resources;
using PROCAS2.Services.App;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ServiceBusMessages;

namespace PROCAS2.Services.Utility
{
    public static class CRAList
    {
        /// <summary>
        /// Add a string to a list if it is not null
        /// </summary>
        /// <param name="list">list of string</param>
        /// <param name="item">item to add</param>
        public static void AddIfNotNull(this List<string> list, string item)
        {
            if (!String.IsNullOrEmpty(item))
            {
                list.Add(item);
            }
        }
    }

    public class CRAService:ICRAService
    {

        private IWebJobParticipantService _participantService;
        private IResponseService _responseService;
        private IConfigService _configService;
        private IWebJobLogger _logger;

       

        public CRAService(IWebJobParticipantService participantService,
                            IResponseService responseService,
                            IConfigService configService,
                            IWebJobLogger logger)
        {
            _participantService = participantService;
            _responseService = responseService;
            _configService = configService;
            _logger = logger;
        }



        


        /// <summary>
        /// Process and create the consent from the incoming HL7 consent message.
        /// </summary>
        /// <param name="consentMessage">The message</param>
        /// <returns>List of errors (empty if no errors!)</returns>
        public List<string> ProcessConsent(string consentMessage)
        {
            List<string> retMessages = new List<string>();
            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Info, "*** PROCESS CONSENT *** "));

            

           // try
            //{
                // Get the consent message
                ConsentMessage consentObj = JsonConvert.DeserializeObject<ConsentMessage>(consentMessage);
                if (consentObj != null && consentObj.IsValid)
                {
                    // Patient has to exist!
                    if (_participantService.DoesHashedNHSNumberExist(consentObj.PatientId) == false)
                    {
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, String.Format(HL7Resources.PATIENT_NOT_EXISTS, consentObj.PatientId), messageBody: consentMessage));
                        return retMessages;
                    }

                    // Set the consent flag and date in the DB
                    if (_participantService.SetConsentFlag(consentObj.PatientId) == false)
                    {
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, String.Format(HL7Resources.CONSENT_NOT_SET, consentObj.PatientId), messageBody: consentMessage));
                    }

                    // Move the PDF to storage
                    DateTime now = DateTime.Now;
                    string filename = _participantService.GetStudyNumber(consentObj.PatientId) + "-" + now.ToString("yyyy-MM-dd-hh-mm-ss") + ".pdf";
                    if (ProcessConsentPDF(consentObj.ConsentPDF, filename) == false)
                    {
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, HL7Resources.CONSENT_PDF_ERROR, messageBody: consentMessage));
                    }

                    // Post message on Volpara outgoing queue - to inform them of consent
                    string message = @"{ 'patientId' : '" + consentObj.PatientId + "'}";
                    if (PostServiceBusMessage(message, "VolparaConsentQueue") == false)
                    {
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, HL7Resources.CONSENT_OUTGOING_ERROR, messageBody: consentMessage));
                    }

                }
                else
                {
                    retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, HL7Resources.CONSENT_MESSAGE_FORMAT_INVALID, messageBody: consentMessage)); 
                }
           // }
            //catch
           // {
            //    retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, HL7Resources.CONSENT_MESSAGE_FORMAT_INVALID, messageBody: consentMessage)); 
           // }

            return retMessages;
        }

        /// <summary>
        /// Process and create the questionnaire from the incoming HL7 ORU^R01 message.
        /// </summary>
        /// <param name="hl7Message">The message</param>
        /// <returns>List of errors (empty if no errors!)</returns>
        public List<string> ProcessQuestionnaire(string hl7Message)
        {
            List<string> retMessages = new List<string>();

            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Info, "*** PROCESS QUESTIONNAIRE *** "));
           
           

            PipeParser parser = new PipeParser();
            IMessage m;
            try
            {
                m = parser.Parse(hl7Message);
            }
            catch
            {
                retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, HL7Resources.NOT_ORUR01, messageBody: hl7Message));
                return retMessages;
            }
            

            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Info, "Parsed message"));
            Terser terse = new Terser(m);

            string messageTypePart1 = terse.Get("MSH-9-1");
            
            string messageTypePart2 = terse.Get("MSH-9-2");
            
            // Must be an ORU^R01 message
            if (messageTypePart1 != "ORU" || messageTypePart2 != "R01")
            {
                retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, HL7Resources.NOT_ORUR01, messageBody: hl7Message));
                
                return retMessages;
            }

            ORU_R01 ORUR01 = m as ORU_R01; // If you are wondering why this is necessary, so am I. Try taking it out to find out why it is here. Go on, I dare you.
            
            
            // Get and validate the patient ID.
            string patientID = terse.Get("/.^PATIENT$/PID-3");
            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Info, "Got patient ID "));
            if (String.IsNullOrEmpty(patientID) || _participantService.DoesHashedNHSNumberExist(patientID) == false)
            {
                retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, String.Format(HL7Resources.PATIENT_NOT_EXISTS, patientID), messageBody: hl7Message));
                return retMessages;
            }

            // Get the questionnaire start and end date
            string dateStarted = terse.Get("/.^ORDER_OBSERVATION$(0)/OBR-7");
           
            string dateFinished = terse.Get("/.^ORDER_OBSERVATION$(0)/OBR-8");
            

            // Create the questionnaire response header
            QuestionnaireResponse response;
            if (_responseService.CreateQuestionnaireHeader(patientID, dateStarted, dateFinished, out response) == false)
            {
                retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, HL7Resources.HEADER_CREATION_ERROR, messageBody: hl7Message));
                return retMessages;
            }

            List<string> letterParts = new List<string>();
            List<string> historyParts = new List<string>();
            string riskCategory = "";
            string riskScore = "";

            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Info, "Processing the observation records"));
            // Cycle through the observation records, pulling out the ones of interest
            bool stop = false;
            int idxOBX = -1;
            do
            {
                idxOBX++;
                // Get the type of each OBX record
                string observationType = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-3-1");
                
                if (String.IsNullOrEmpty(observationType))
                {
                    // No more OBX records
                    stop = true;
                }
                else
                {
                    

                    // Is this observation a risk letter?
                    if (observationType == _configService.GetAppSetting("HL7RiskLetterCode"))
                    {
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Info, "Risk Letter"));
                        int idxLetter = 0;
                        bool letterStop = false;
                        bool firstNull = true;
                        // iterate through the letter and store each paragraph
                        do
                        {
                            string answerText = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5(" + idxLetter + ")");
                           
                            if (answerText == null)
                            {
                                if (firstNull == false) // Stop on second blank line in a row.
                                {
                                    letterStop = true;
                                }
                                else
                                {
                                    letterParts.Add("");
                                    firstNull = false;
                                    idxLetter++;
                                }
                            }
                            else
                            {
                                firstNull = true;
                                letterParts.Add(answerText);
                                idxLetter++;
                            }
                        } while (letterStop == false);


                        continue;
                    }

                    // Is this observation a risk score?
                    if (observationType == _configService.GetAppSetting("HL7RiskScoreCode"))
                    {
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Info, "Risk Score"));
                        riskScore = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5-1");
                        continue;
                    }

                    // Is this observation a risk category?
                    if (observationType == _configService.GetAppSetting("HL7RiskCategoryCode"))
                    {
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Info, "Risk Category"));
                        riskCategory = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5-1");
                        continue;
                    }

                    // Is the observation a family history type?
                    if (observationType == _configService.GetAppSetting("HL7FamilyHistoryCode"))
                    {
                        int idxHistory = 0;
                        bool historyStop = false;
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Info, "Family History"));
                        // iterate through the family history and store each record
                        do
                        {
                            string historyCode = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5(" + idxHistory + ")");
                            
                            if (historyCode == null)
                            {
                                
                                    historyStop = true;
                               
                            }
                            else
                            {
                                // Pick out the family history record.
                               
                                historyParts.Add(historyCode);
                                FamilyHistoryItem historyItem = new FamilyHistoryItem();
                                historyItem.RelationshipCode = historyCode;
                                historyItem.RelationshipDescription = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5(" + idxHistory + ")-2");
                                
                                historyItem.Gender = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5(" + idxHistory + ")-3");
                                
                                string age = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5(" + idxHistory + ")-4");
                                
                                if (String.IsNullOrEmpty(age))
                                {
                                    historyItem.Age = null;
                                }
                                else
                                {
                                    historyItem.Age = Convert.ToInt32(age);
                                }

                                historyItem.Disease = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5(" + idxHistory + ")-5-2");
                                

                                string ageOfDiagnosis = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5(" + idxHistory + ")-6");
                                
                                if (String.IsNullOrEmpty(ageOfDiagnosis))
                                {
                                    historyItem.AgeOfDiagnosis = null;
                                }
                                else
                                {
                                    historyItem.AgeOfDiagnosis = Convert.ToInt32(ageOfDiagnosis);
                                }

                                // Create the record in the DB
                                if (_responseService.CreateFamilyHistoryItem(response, historyItem) == false)
                                {
                                    retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, String.Format(HL7Resources.FAMILY_HISTORY_ERROR, patientID), messageBody: hl7Message));
                                }

                                idxHistory++;
                            }
                        } while (historyStop == false);
                    }

                    // Is the observation a consent type?
                    if (observationType == _configService.GetAppSetting("HL7ConsentCode"))
                    {
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Info, "Consent"));
                        string answerText = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5-1");
                        
                        if (answerText.ToLower() == "yes")
                        {
                            if(_participantService.SetConsentFlag(patientID) == false)
                            {
                                retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, String.Format(HL7Resources.CONSENT_NOT_SET, patientID), messageBody: hl7Message));
                            }
                        }

                        continue;
                    }

                    // Is the observation a survey question type?
                    if (observationType.StartsWith(_configService.GetAppSetting("HL7SurveyQuestionCode")))
                    {
                        retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Info, "Survey Question"));
                        string[] splitType = observationType.Split('.'); // the type is of format: <typeCode>.<questionCode>
                        string answerText = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5-1");
                        
                        if (_responseService.CreateResponseItem(response, splitType[1], answerText) == false)
                        {
                            retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, String.Format(HL7Resources.OBX_ERROR, observationType), messageBody: hl7Message));
                        }

                        continue;
                    }

                    
                }
                
            } while (stop == false);


            // If there are any paragraphs in the letter then create a risk letter for the participant
            if (letterParts.Count > 0)
            {
                if (_participantService.CreateRiskLetter(patientID, riskScore, riskCategory, letterParts) == false)
                {
                    retMessages.AddIfNotNull(_logger.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, String.Format(HL7Resources.RISK_LETTER_NOT_CREATED, patientID), messageBody: hl7Message));
                }
            }

            

            return retMessages;
        }


        /// <summary>
        /// Is the passed message a consent message? If not, it is an HL7 message.
        /// </summary>
        /// <param name="message">the message</param>
        /// <returns>true if a consent message, else false</returns>
        public bool IsConsentMessage(string message)
        {
            try
            {
                ConsentMessage consentObj = JsonConvert.DeserializeObject<ConsentMessage>(message);

                return consentObj.IsValid;
            }
            catch
            {
                return false;
            }

        }


        /// <summary>
        /// Create and store the consent PDF in Azure storage
        /// </summary>
        /// <param name="PDF">Base 64 encoded PDF</param>
        /// <param name="filename">File name to save the consent form as</param>
        /// <returns>true if successful, else false</returns>
        public bool ProcessConsentPDF(string PDF, string filename)
        {
            try
            {
                byte[] sPDFDecoded = Convert.FromBase64String(PDF);
               
                MemoryStream stream = new MemoryStream(sPDFDecoded);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configService.GetConnectionString("CollatorPrimaryStorage"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_configService.GetAppSetting("StorageConsentContainer"));

                var blob = container.GetBlockBlobReference(filename);
                blob.UploadFromStreamAsync(stream).Wait();
                stream.Dispose();

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Post a message to the CRA servicebus. Used for testing mainly!
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>true if successfully posted, else false</returns>
        public bool PostServiceBusMessage(string message, string queue)
        {
            try
            {
                string keyName = _configService.GetAppSetting("CRA-ServiceBusKeyName");
                string keyValue = _configService.GetAppSetting("CRA-ServiceBusKeyValue");

                TokenProvider credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, keyValue);

                // Create a URI for the serivce bus.
                Uri serviceBusUri = ServiceBusEnvironment.CreateServiceUri
                    ("sb", _configService.GetAppSetting("CRA-ServiceBusBase"), string.Empty);

                // Create a message factory for the service bus URI using the
                // credentials
                MessagingFactory factory = MessagingFactory.Create(serviceBusUri, credentials);

                // Create a queue client 
                QueueClient queueClient =
                    factory.CreateQueueClient(_configService.GetAppSetting(queue));

                // Post in Base64 encoded form (as seems to be common practice)
                BrokeredMessage hl7Message = new BrokeredMessage(Encoding.UTF8.GetBytes(message));

                // Send the message to the queue.
                queueClient.Send(hl7Message);

                factory.Close();

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }


        public string GetServiceBusMessage(string queue)
        {
            string keyName = _configService.GetAppSetting("CRA-ServiceBusKeyName");
            string keyValue = _configService.GetAppSetting("CRA-ServiceBusKeyValue");

            TokenProvider credentials =
               TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, keyValue);

            // Create a URI for the serivce bus.

            Uri serviceBusUri = ServiceBusEnvironment.CreateServiceUri
                ("sb", _configService.GetAppSetting("CRA-ServiceBusBase"), string.Empty);

            // Create a message factory for the service bus URI using the
            // credentials
            MessagingFactory factory = MessagingFactory.Create(serviceBusUri, credentials);

            // Create a queue client
            QueueClient queueClient =
                factory.CreateQueueClient(_configService.GetAppSetting(queue));

            BrokeredMessage orderOutMsg = queueClient.Receive();

            string message = "";

            if (orderOutMsg != null)
            {
                message = System.Text.Encoding.UTF8.GetString(orderOutMsg.GetBody<byte[]>());
                orderOutMsg.Complete();
            }

            factory.Close();
            return message;

        }

    }
}
