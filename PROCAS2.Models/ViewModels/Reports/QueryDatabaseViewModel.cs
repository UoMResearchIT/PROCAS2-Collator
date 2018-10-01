using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using PROCAS2.Data.Entities;
using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels.Reports
{
    public class QueryDatabaseViewModel
    {
        [Display(Name = "QDB_ANONYMISE", ResourceType = typeof(ReportResources))]
        [Required]
        public bool Anonymise { get; set; }

        [Display(Name = "QDB_INCLUDE_ADDRESSES", ResourceType = typeof(ReportResources))]
        [Required]
        public bool IncludeAddresses { get; set; }

        [Display(Name = "QDB_INCLUDE_HISTOLOGY", ResourceType = typeof(ReportResources))]
        [Required]
        public bool IncludeHistology { get; set; }

        [Display(Name = "QDB_MR_QUESTIONNAIRE", ResourceType = typeof(ReportResources))]
        [Required]
        public bool OnlyMostRecentQuestionnaire { get; set; }

        [Display(Name = "QDB_MR_RISK", ResourceType = typeof(ReportResources))]
        [Required]
        public bool OnlyMostRecentRisk { get; set; }

        [Display(Name = "QDB_MR_VOLPARA", ResourceType = typeof(ReportResources))]
        [Required]
        public bool OnlyMostRecentVolpara { get; set; }

        [Display(Name = "QDB_CONSENTED_FROM", ResourceType = typeof(ReportResources))]
        [Required]
        public DateTime ConsentedFrom { get; set; }

        [Display(Name = "QDB_CONSENTED_TO", ResourceType = typeof(ReportResources))]
        [Required]
        public DateTime ConsentedTo { get; set; }

        [Display(Name = "QDB_SCREENING_SITE", ResourceType = typeof(ReportResources))]
        [Required]
        public string ScreeningSite { get; set; }

        public List<ScreeningSite> ScreeningSites { get; set; }
    }
}
