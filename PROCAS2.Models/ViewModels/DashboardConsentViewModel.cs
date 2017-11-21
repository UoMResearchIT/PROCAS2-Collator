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
        [Display(Name = "VM_DASHBOARD_NUM_PARC", ResourceType = typeof(PROCASRes))]
        public int NumberParticipants { get; set; }
        [Display(Name = "VM_DASHBOARD_NUM_CONSENT", ResourceType = typeof(PROCASRes))]
        public int NumberConsented { get; set; }
        [Display(Name = "VM_DASHBOARD_NUM_YET_CONSENT", ResourceType = typeof(PROCASRes))]
        public int NumberYetToConsent { get; set; }
        [Display(Name = "VM_DASHBOARD_NUM_CONSENT_NO_DETAILS", ResourceType = typeof(PROCASRes))]
        public int NumberConsentedNoDetails { get; set; }
    }
}
