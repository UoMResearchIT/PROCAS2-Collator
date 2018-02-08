using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

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
        public  void ProcessQueueMessage([ServiceBusTrigger("cra-incoming")] string message, TextWriter log)
        {
            //log.WriteLine(message);
            _craService._logFile = log;
            List<string> messages = _craService.ProcessQuestionnaire(message);
            
            foreach(string mess in messages)
            {
                log.WriteLine(mess);
            }
        }
    }
}
