using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using PROCAS2.Services.Utility;

namespace PROCAS2.Webjob.GetCRAConsent
{
    public class Functions
    {
       
        private ICRAService _craService;

        public Functions(ICRAService craService)
        {
           
            _craService = craService;
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure ServiceBus queue called cra-consent-incoming-test.
        public  void ProcessConsentMessage([ServiceBusTrigger("craconsentqueue")] BrokeredMessage message, TraceWriter log)
        {

            string messageStr = System.Text.Encoding.UTF8.GetString(message.GetBody<byte[]>());

            // There are two types of message on this queue. If this message is not a consent message then
            // assume that it is an HL7 message
            if (_craService.IsConsentMessage(messageStr) == true)
            {
                
           
                // This is a consent message

                List<string> messages = _craService.ProcessConsent(messageStr);

                //foreach (string mess in messages)
                //{
                //    log.Warning(mess);
                //}
            }

           
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure ServiceBus queue called cra-suvey-incoming-test.
        //public void ProcessSurveyMessage([ServiceBusTrigger("cra-survey-incoming-test")] BrokeredMessage message, TraceWriter log)
        //{
        //    string messageStr = System.Text.Encoding.UTF8.GetString(message.GetBody<byte[]>());

        //    List<string> messages = _craService.ProcessQuestionnaire(messageStr);
        //}


    }
}
