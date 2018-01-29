using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Services.Utility
{
    public interface ICRAService
    {
        List<string> ProcessQuestionnaire(string hl7Message);

        bool PostServiceBusMessage(string message);
        string GetServiceBusMessage();
    }
}
