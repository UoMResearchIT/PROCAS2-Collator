using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Models.ServiceBusMessages
{
    /// <summary>
    /// Structure of the JSON consent message coming from CRA
    /// </summary>
    public class ConsentMessage
    {
        public string MessageType { get; set; }

        public string MessageTimeStamp { get; set; }

        public string PatientId { get; set; }

    }
}
