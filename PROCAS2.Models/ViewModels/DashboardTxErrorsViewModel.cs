using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class DashboardTxErrorsViewModel
    {

        [Display(Name = "CRA_CONSENT_ERRORS", ResourceType = typeof(DashboardResources))]
        public int CRAConsentErrors { get; set; }

        [Display(Name = "CRA_SURVEY_ERRORS", ResourceType = typeof(DashboardResources))]
        public int CRASurveyErrors { get; set; }
    }
}
