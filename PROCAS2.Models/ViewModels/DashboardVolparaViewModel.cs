using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class DashboardVolparaViewModel
    {
        [Display(Name = "NUM_RISK_NOT_ASKED_FOR", ResourceType = typeof(DashboardResources))]
        public int NumberLetterNotAskedFor { get; set; }
        [Display(Name = "NUM_CONSENTED_NO_VOLPARA", ResourceType = typeof(DashboardResources))]
        public int NumberWaitingForVolpara { get; set; }
        [Display(Name = "NUM_CONSENTED_NO_VOLPARA_NEAR_6WEEKS", ResourceType = typeof(DashboardResources))]
        public int NumberWaitingForVolparaNear6Weeks { get; set; }
    }
}
