using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace PROCAS2.Services.Utility
{
    public interface ICRAService
    {

        
        List<string> ProcessQuestionnaire(string hl7Message);
        List<string> ProcessConsent(string consentMessage);

        bool IsConsentMessage(string message);

        bool PostServiceBusMessage(string message);
        string GetServiceBusMessage();

        TextWriter _logFile { get; set; }
    }
}
