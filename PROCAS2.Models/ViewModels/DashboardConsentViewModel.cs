using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Models.ViewModels
{
    public class DashboardConsentViewModel
    {
        public int NumberParticipants { get; set; }
        public int NumberConsented { get; set; }
        public int NumberYetToConsent { get; set; }
        public int NumberConsentedNoDetails { get; set; }
    }
}
