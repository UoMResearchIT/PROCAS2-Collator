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

namespace PROCAS2.Webjob.GetVolparaMessages
{
    public class Functions
    {
        
        private IVolparaService _volparaService;

        public Functions(IVolparaService volparaService)
        {

            _volparaService = volparaService;
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public void ProcessScreeningMessage([ServiceBusTrigger("volpara-screening-incoming-test")] BrokeredMessage message, TextWriter log)
        {
            string messageStr = System.Text.Encoding.UTF8.GetString(message.GetBody<byte[]>());

            
            

            List<string> messages =  _volparaService.ProcessScreeningMessage(messageStr);

                foreach (string mess in messages)
                {
                    log.WriteLine(mess);
                }
           
        }
    }
}
