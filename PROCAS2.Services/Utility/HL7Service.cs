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

            ORU_R01 ORUR01 = m as ORU_R01;

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

            // Cycle through the observation records, pulling out the ones of interest
            bool stop = false;
            int idxOBX = 0;
            do
            {
                // Get the type of each OBX record
                string observationType = terse.Get("/.^OBSERVATION$(" + idxOBX + ")/OBX-3-1");
                if (String.IsNullOrEmpty(observationType))
                {
                    // No more OBX records
                    stop = true;
                }
                else
                {
                    idxOBX++;

                    // Is this observation a risk letter?
                    if (observationType == _configService.GetAppSetting("HL7RiskLetterCode"))
                    {
                        continue;
                    }

                    // Is the observation a consent type?
                    if (observationType == _configService.GetAppSetting("HL7ConsentCode"))
                    {
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

            //ORU_R01 ORUR01 = m as ORU_R01;

            //var top = ORUR01.GetAll("OBX");
            //foreach (ORU_R01_RESPONSE response in ORUR01.RESPONSEs)
            //{
            //    //var resp = response.GetAll("OBX");
            //    foreach(ORU_R01_ORDER_OBSERVATION obsv in response.ORDER_OBSERVATIONs)
            //    {
            //        ORU_R01_OBSERVATION ob = obsv.GetOBSERVATION(1);
            //        ORU_R01_OBSERVATION ob2 = obsv.GetOBSERVATION(2);
            //        ORU_R01_OBSERVATION ob3 = obsv.GetOBSERVATION(3);

            //        Varies[] observations = ob.OBX.GetObservationValue();
            //    }
            //}

            return returnMessages;
        }

    }
}
