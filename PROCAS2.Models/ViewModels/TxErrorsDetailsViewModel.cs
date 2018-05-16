using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class TxErrorsDetailsViewModel
    {
        [Display(Name="LOG_DATE", ResourceType = typeof(TxErrorsResources))]
        public DateTime LogDate { get; set; }

        
        public int MessageType { get; set; }

       
        public int LogLevel { get; set; }

        [Display(Name = "LOG_MESSAGE", ResourceType = typeof(TxErrorsResources))]
        public string Message { get; set; }

        [Display(Name = "LOG_STACKTRACE", ResourceType = typeof(TxErrorsResources))]
        public string StackTrace { get; set; }

        [Display(Name = "LOG_BODY", ResourceType = typeof(TxErrorsResources))]
        public string MessageBody { get; set; }

        [Display(Name = "LOG_REVIEWED", ResourceType = typeof(TxErrorsResources))]
        public bool Reviewed { get; set; }

        [Display(Name = "LOG_LEVEL", ResourceType = typeof(TxErrorsResources))]
        public string LogLevelString
        {
            get
            {
                switch(LogLevel)
                {
                    case 1:
                        return "INFO";
                    case 2:
                        return "WARNING";
                    default:
                        return "ERROR";
                }
            }
        }

        [Display(Name = "LOG_TYPE", ResourceType = typeof(TxErrorsResources))]
        public string MessageTypeString
        {
            get
            {
                switch(MessageType)
                {
                    case 1:
                        return "CRA Consent";
                    case 3:
                        return "Volpara Screening";
                    default:
                        return "CRA Survey";
                }
            }
        }
    }
}
