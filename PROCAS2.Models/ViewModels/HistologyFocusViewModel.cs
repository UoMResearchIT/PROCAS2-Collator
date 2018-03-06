using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using PROCAS2.Data.Entities;
using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class HistologyFocusViewModel
    {

        public int Id { get; set; }
        public int HistologyId { get; set; }
        public string NHSNumber { get; set; }
        
        public int PrimaryNumber { get; set; }

        [Display(Name = "FOCUS_NUMBER", ResourceType = typeof(HistologyResources))]
        public int FocusNumber { get; set; }

        [Display(Name = "PATHOLOGY", ResourceType = typeof(HistologyResources))]
        public int? PathologyId { get; set; }

        [Display(Name = "INVASIVE_CIS", ResourceType = typeof(HistologyResources))]
        public int? InvasiveId { get; set; }

        [Display(Name = "INVASIVE_TUMOUR_SIZE", ResourceType = typeof(HistologyResources))]
        public double? InvasiveTumourSize { get; set; }

        [Display(Name = "WHOLE_TUMOUR_SIZE", ResourceType = typeof(HistologyResources))]
        public double? WholeTumourSize { get; set; }

        [Display(Name = "INVASIVE_GRADE", ResourceType = typeof(HistologyResources))]
        public int? InvasiveGrade { get; set; }

        [Display(Name = "DCIS_GRADE", ResourceType = typeof(HistologyResources))]
        public int? DCISGradeId { get; set; }

        [MaxLength(10)]
        [Display(Name = "LYMPH_NODES", ResourceType = typeof(HistologyResources))]
        public string LymphNodes { get; set; }

        [Display(Name = "LN2", ResourceType = typeof(HistologyResources))]
        public int LN2 { get; set; }

        [Display(Name = "VASCULAR_INVASION", ResourceType = typeof(HistologyResources))]
        public int? VascularInvasionId { get; set; }

        [Display(Name = "ER_STATUS", ResourceType = typeof(HistologyResources))]
        public bool? ERStatus { get; set; }

        [Display(Name = "ER_SCORE", ResourceType = typeof(HistologyResources))]
        public int? ERScore { get; set; }

        [Display(Name = "PR_STATUS", ResourceType = typeof(HistologyResources))]
        public bool? PRStatus { get; set; }

        [Display(Name = "PR_SCORE", ResourceType = typeof(HistologyResources))]
        public int? PRScore { get; set; }

        [Display(Name = "HER2_STATUS", ResourceType = typeof(HistologyResources))]
        public bool? HER2Status { get; set; }

        [MaxLength(30)]
        [Display(Name = "HER2_SCORE", ResourceType = typeof(HistologyResources))]
        public string HER2Score { get; set; }

        [Display(Name = "KI_67", ResourceType = typeof(HistologyResources))]
        public double? KISixtySeven { get; set; }


        public List<HistologyLookup> VascularInvasions { get; set; }
        public List<HistologyLookup> DCISGrades { get; set; }
        public List<HistologyLookup> Invasives { get; set; }
        public List<HistologyLookup> Pathologies { get; set; }
    }
}
