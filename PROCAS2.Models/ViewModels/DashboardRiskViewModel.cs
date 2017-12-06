using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class DashboardRiskViewModel
    {
        [Display(Name = "NUM_RISK_WAITING", ResourceType = typeof(DashboardResources))]
        public int NumberWaitingForLetter { get; set; }
        [Display(Name = "NUM_RISK_NOT_SENT", ResourceType = typeof(DashboardResources))]
        public int NumberLetterNotSent { get; set; }
        
    }
}
