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
        public bool PostServiceBusMessage(string message, string queue)
        {
            try
            {
                string keyName = _configService.GetAppSetting("CRA-ServiceBusKeyName");
                string keyValue = _configService.GetAppSetting("CRA-ServiceBusKeyValue");

                TokenProvider credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, keyValue);

                // Create a URI for the serivce bus.
                Uri serviceBusUri = ServiceBusEnvironment.CreateServiceUri
                    ("sb", _configService.GetAppSetting("CRA-ServiceBusBase"), string.Empty);

                // Create a message factory for the service bus URI using the
                // credentials
                MessagingFactory factory = MessagingFactory.Create(serviceBusUri, credentials);

                // Create a queue client 
                QueueClient queueClient =
                    factory.CreateQueueClient(_configService.GetAppSetting(queue));

                // Post in Base64 encoded form (as seems to be common practice)
                BrokeredMessage hl7Message = new BrokeredMessage(Encoding.UTF8.GetBytes(message));

                // Send the message to the queue.
                queueClient.Send(hl7Message);

                factory.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public string GetServiceBusMessage(string queue)
        {
            string keyName = _configService.GetAppSetting("CRA-ServiceBusKeyName");
            string keyValue = _configService.GetAppSetting("CRA-ServiceBusKeyValue");

            TokenProvider credentials =
               TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, keyValue);

            // Create a URI for the serivce bus.

            Uri serviceBusUri = ServiceBusEnvironment.CreateServiceUri
                ("sb", _configService.GetAppSetting("CRA-ServiceBusBase"), string.Empty);

            // Create a message factory for the service bus URI using the
            // credentials
            MessagingFactory factory = MessagingFactory.Create(serviceBusUri, credentials);

            // Create a queue client
            QueueClient queueClient =
                factory.CreateQueueClient(_configService.GetAppSetting(queue));

            BrokeredMessage orderOutMsg = queueClient.Receive();

            string message = "";

            if (orderOutMsg != null)
            {
                message = System.Text.Encoding.UTF8.GetString(orderOutMsg.GetBody<byte[]>());
                orderOutMsg.Complete();
            }

            factory.Close();
            return message;

        }
    }
}
