using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace PROCAS2.Services.Utility
{
    public class ServiceBusService:IServiceBusService
    {

        private IConfigService _configService;


        public ServiceBusService(IConfigService configService)
        {
            _configService = configService;
        }


        /// <summary>
        /// Post a message to the CRA servicebus. Used for testing mainly!
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>true if successfully posted, else false</returns>
        public bool PostServiceBusMessage(string keyNameAppSetting, string keyValueAppSetting, string baseURLAppSetting, string message, string queueAppSetting, bool compress)
        {
            try
            {
                string keyName = _configService.GetAppSetting(keyNameAppSetting);
                string keyValue = _configService.GetAppSetting(keyValueAppSetting);

                TokenProvider credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, keyValue);

                // Create a URI for the serivce bus.
                Uri serviceBusUri = ServiceBusEnvironment.CreateServiceUri
                    ("sb", _configService.GetAppSetting(baseURLAppSetting), string.Empty);

                // Create a message factory for the service bus URI using the
                // credentials
                MessagingFactory factory = MessagingFactory.Create(serviceBusUri, credentials);

                // Create a queue client 
                QueueClient queueClient =
                    factory.CreateQueueClient(_configService.GetAppSetting(queueAppSetting));

                // Post in Base64 encoded form (as seems to be common practice)
                if (compress == true)
                {
                    BrokeredMessage hl7Message = new BrokeredMessage(GzipCompressor.Compress(message));
                    hl7Message.ContentType = "application/gzip";
                    // Send the message to the queue.
                    queueClient.Send(hl7Message);
                }
                else
                {
                    BrokeredMessage hl7Message = new BrokeredMessage(Encoding.UTF8.GetBytes(message));
                    // Send the message to the queue.
                    queueClient.Send(hl7Message);
                }
                factory.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public string GetServiceBusMessage(string keyNameAppSetting, string keyValueAppSetting, string baseURLAppSetting, string queueAppSetting)
        {
            string keyName = _configService.GetAppSetting(keyNameAppSetting);
            string keyValue = _configService.GetAppSetting(keyValueAppSetting);

            TokenProvider credentials =
               TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, keyValue);

            // Create a URI for the serivce bus.

            Uri serviceBusUri = ServiceBusEnvironment.CreateServiceUri
                ("sb", _configService.GetAppSetting(baseURLAppSetting), string.Empty);

            // Create a message factory for the service bus URI using the
            // credentials
            MessagingFactory factory = MessagingFactory.Create(serviceBusUri, credentials);

            // Create a queue client
            QueueClient queueClient =
                factory.CreateQueueClient(_configService.GetAppSetting(queueAppSetting));

            BrokeredMessage orderOutMsg = queueClient.Receive();

            string message = "";
            var myInputBytes = orderOutMsg.GetBody<byte[]>();
            if (orderOutMsg != null)
            {
                switch (orderOutMsg.ContentType)
                {
                    case "application/gzip":
                        message = GzipCompressor.Decompress(myInputBytes);
                        break;
                    case "application/json":
                        message = Encoding.UTF8.GetString(myInputBytes);
                        break;
                    default:            
                        message = Encoding.UTF8.GetString(myInputBytes);
                        break;

                }
                
                orderOutMsg.Complete();
            }

            factory.Close();
            return message;

        }
    }
}
