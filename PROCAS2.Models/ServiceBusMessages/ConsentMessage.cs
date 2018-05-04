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
        public string MessageType { get; set; } // should always be 'consent'

        public string PatientId { get; set; } // hashed patient ID

        public string ConsentPDF { get; set; } // Base64 encoded consent PDF form. For the moment we need to allow nulls here as the functionality won't be in place at launch.

        // TODO: Check for null PDF (after CRA functionality in place!)
        public bool IsValid { get
            {
                return !String.IsNullOrEmpty(MessageType) && 
                        !String.IsNullOrEmpty(PatientId) &&
                        MessageType == "consent";
            }

        }

    }
}
