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
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessScreeningMessage([ServiceBusTrigger("volpara-consent-incoming-test")] BrokeredMessage message, TextWriter log)
        {
            string messageStr = System.Text.Encoding.UTF8.GetString(message.GetBody<byte[]>());
        }
    }
}
