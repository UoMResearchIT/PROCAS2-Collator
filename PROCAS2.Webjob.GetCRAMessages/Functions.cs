using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

using PROCAS2.Services.Utility;

namespace PROCAS2.Webjob.GetCRAMessages
{
    public class Functions
    {
       
        private ICRAService _craService;

        public Functions(ICRAService craService)
        {
           
            _craService = craService;
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure ServiceBus queue called cra-incoming.
        public  void ProcessQueueMessage([ServiceBusTrigger("cra-incoming")] string message, TraceWriter log)
        {
            
            _craService._logFile = null;
            // There are two types of message on this queue. If this message is not a consent message then
            // assume that it is an HL7 message
            if (_craService.IsConsentMessage(message) == false)
            {
                List<string> messages = _craService.ProcessQuestionnaire(message);

                foreach (string mess in messages)
                {
                    log.Trace(new TraceEvent(System.Diagnostics.TraceLevel.Warning, mess));
                }
            }
            else
            {
                // This is a consent message

                List<string> messages = _craService.ProcessConsent(message);

                foreach (string mess in messages)
                {
                    log.Warning(mess);
                }
            }

           
        }

       
    }
}
