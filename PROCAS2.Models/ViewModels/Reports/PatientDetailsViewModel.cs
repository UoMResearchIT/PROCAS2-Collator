using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels.Reports
{
    public class PatientDetailsViewModel
    {
        [Display(Name="NHS_NUMBER", ResourceType = typeof(ReportResources))]
        public string NHSNumber { get; set; }
    }
}
