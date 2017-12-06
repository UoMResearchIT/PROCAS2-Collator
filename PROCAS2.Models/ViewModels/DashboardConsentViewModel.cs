using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class DashboardConsentViewModel
    {
        [Display(Name = "NUM_PARC", ResourceType = typeof(DashboardResources))]
        public int NumberParticipants { get; set; }
        [Display(Name = "NUM_CONSENT", ResourceType = typeof(DashboardResources))]
        public int NumberConsented { get; set; }
        [Display(Name = "NUM_YET_CONSENT", ResourceType = typeof(DashboardResources))]
        public int NumberYetToConsent { get; set; }
        [Display(Name = "NUM_CONSENT_NO_DETAILS", ResourceType = typeof(DashboardResources))]
        public int NumberConsentedNoDetails { get; set; }
    }
}
