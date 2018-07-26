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
        [Singleton]
        public void ProcessScreeningMessage([ServiceBusTrigger("procasstudyresultfornhsqueue")] BrokeredMessage message, TextWriter log)
        {

            string messageStr = "";

            switch (message.ContentType)
            {
                case "application/gzip":
                    using (var myStream = message.GetBody<Stream>())
                    {
                        using (var myMemoryStream = new MemoryStream())
                        {
                            myStream.CopyTo(myMemoryStream);
                            messageStr = GzipCompressor.Decompress(myMemoryStream.ToArray());
                        }
                    }
                    break;
                case "application/json":
                    // Use json deserialization
                    using (var myStream = message.GetBody<Stream>())
                    {
                        var myReader = new StreamReader(myStream, Encoding.UTF8);
                        messageStr = myReader.ReadToEnd();
                    }
                    break;
                default:
                    // Use json deserialization
                    using (var myStream = message.GetBody<Stream>())
                    {
                        var myReader = new StreamReader(myStream, Encoding.UTF8);
                        messageStr = myReader.ReadToEnd();
                    }
                    break;

            }

            List<string> messages = _volparaService.ProcessScreeningMessage(messageStr);

            foreach (string mess in messages)
            {
                log.WriteLine(mess);
            }

        }
    }
}
