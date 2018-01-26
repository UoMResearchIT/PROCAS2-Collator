using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NHapi.Base;
using NHapi.Base.Parser;
using NHapi.Base.Util;
using NHapi.Base.SourceGeneration;
using NHapi.Base.Model;
using NHapi.Model.V23.Datatype;
using NHapi.Model.V23.Group;
using NHapi.Model.V23.Segment;
using NHapi.Model.V23.Message;

using PROCAS2.Resources;
using PROCAS2.Services.App;
using PROCAS2.Data.Entities;

namespace PROCAS2.Services.Utility
{
    public class HL7Service:IHL7Service
    {

        private IParticipantService _participantService;
        private IResponseService _responseService;
        private IConfigService _configService;

        public HL7Service(IParticipantService participantService,
                            IResponseService responseService,
                            IConfigService configService)
        {
            _participantService = participantService;
            _responseService = responseService;
            _configService = configService;
        }

        /// <summary>
        /// Process and create the questionnaire from the incoming HL7 ORU^R01 message.
        /// </summary>
        /// <param name="hl7Message">The message</param>
        /// <returns>List of errors (empty if no errors!)</returns>
        public List<string> ProcessQuestionnaire(string hl7Message)
        {

            List<string> returnMessages = new List<string>();

            PipeParser parser = new PipeParser();
           
            IMessage m = parser.Parse(hl7Message);
            Terser terse = new Terser(m);

            string messageTypePart1 = terse.Get("MSH-9-1");
            string messageTypePart2 = terse.Get("MSH-9-2");

            // Must be an ORU^R01 message
            if (messageTypePart1 != "ORU" || messageTypePart2 != "R01")
            {
                returnMessages.Add(HL7Resources.NOT_ORUR01);
                return returnMessages;
            }

            ORU_R01 ORUR01 = m as ORU_R01; // If you are wondering why this is necessary, so am I. Try taking it out to find out why it is here. Go on, I dare you.

            // Get and validate the patient ID.
            string patientID = terse.Get("/.^PATIENT$/PID-3");
            if (String.IsNullOrEmpty(patientID) || _participantService.DoesHashedNHSNumberExist(patientID) == false)
            {
                returnMessages.Add(String.Format(HL7Resources.PATIENT_NOT_EXISTS, patientID));
                return returnMessages;
            }

            // Get the questionnaire start and end date
            string dateStarted = terse.Get("/.^ORDER_OBSERVATION$(0)/OBR-7");
            string dateFinished = terse.Get("/.^ORDER_OBSERVATION$(0)/OBR-8");

            // Create the questionnaire response header
            QuestionnaireResponse response;
            if (_responseService.CreateQuestionnaireHeader(patientID, dateStarted, dateFinished, out response) == false)
            {
                returnMessages.Add(HL7Resources.HEADER_CREATION_ERROR);
                return returnMessages;
            }

            List<string> letterParts = new List<string>();
            List<string> historyParts = new List<string>();
            string riskCategory = "";
            string riskScore = "";

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
                        riskScore = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5-1");
                        continue;
                    }

                    // Is this observation a risk category?
                    if (observationType == _configService.GetAppSetting("HL7RiskCategoryCode"))
                    {
                        riskCategory = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5-1");
                        continue;
                    }

                    // Is the observation a family history type?
                    if (observationType == _configService.GetAppSetting("HL7FamilyHistoryCode"))
                    {
                        int idxHistory = 0;
                        bool historyStop = false;
                       
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
                                    returnMessages.Add(String.Format(HL7Resources.FAMILY_HISTORY_ERROR, patientID));
                                }

                                idxHistory++;
                            }
                        } while (historyStop == false);
                    }

                    // Is the observation a consent type?
                    if (observationType == _configService.GetAppSetting("HL7ConsentCode"))
                    {
                        string answerText = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5-1");
                        if (answerText.ToLower() == "yes")
                        {
                            if(_participantService.SetConsentFlag(patientID) == false)
                            {
                                returnMessages.Add(String.Format(HL7Resources.CONSENT_NOT_SET, patientID));
                            }
                        }

                        continue;
                    }

                    // Is the observation a survey question type?
                    if (observationType.StartsWith(_configService.GetAppSetting("HL7SurveyQuestionCode")))
                    {
                        string[] splitType = observationType.Split('.'); // the type is of format: <typeCode>.<questionCode>
                        string answerText = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-5-1");

                        if (_responseService.CreateResponseItem(response, splitType[1], answerText) == false)
                        {
                            returnMessages.Add(String.Format(HL7Resources.OBX_ERROR, observationType));
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
                    returnMessages.Add(String.Format(HL7Resources.RISK_LETTER_NOT_CREATED, patientID));
                }
            }

            

            return returnMessages;
        }

    }
}
