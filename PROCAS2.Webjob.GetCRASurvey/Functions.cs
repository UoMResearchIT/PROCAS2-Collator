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

namespace PROCAS2.Webjob.GetCRASurvey
{
    public class Functions
    {

        private ICRAService _craService;

        public Functions(ICRAService craService)
        {

            _craService = craService;
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure ServiceBus queue called cra-suvey-incoming-test.
        [Singleton]
        public void ProcessSurveyMessage([ServiceBusTrigger("craresultqueue")] BrokeredMessage message, TraceWriter log)
        {
            string messageStr = System.Text.Encoding.UTF8.GetString(message.GetBody<byte[]>());

            List<string> messages = _craService.ProcessQuestionnaire(messageStr);
        }
    }
}
