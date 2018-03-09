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
    public class HistologyEditViewModel
    {

        public HistologyEditViewModel()
        {
            HeaderId = 0;
            fromSave = false;
            HistologyFoci = new List<HistologyFocus>();
        }

        public string NHSNumber { get; set; }
        public int PrimaryNumber { get; set; }

        [Display(Name ="DIAGNOSIS_TYPE", ResourceType =typeof(HistologyResources))]
        public int? DiagnosisTypeId { get; set; }

        [Display(Name = "DIAGNOSIS_DATE", ResourceType = typeof(HistologyResources))]
        public DateTime? DiagnosisDate { get; set; }

        [Display(Name = "MAMMOGRAM_DATE", ResourceType = typeof(HistologyResources))]
        public DateTime? MammogramDate { get; set; }

        [Display(Name = "DIAGNOSIS_SIDE", ResourceType = typeof(HistologyResources))]
        public int? DiagnosisSideId { get; set; }

        [Display(Name = "DIAGNOSIS_MULTI_FOCAL", ResourceType = typeof(HistologyResources))]
        public bool DiagnosisMultiFocal { get; set; }

        [Display(Name = "COMMENTS", ResourceType = typeof(HistologyResources))]
        public string Comments { get; set; }

        public List<HistologyLookup> DiagnosisTypes { get; set; }
        public List<HistologyLookup> DiagnosisSides { get; set; } 
        public List<HistologyFocus> HistologyFoci { get; set; }

        public bool fromSave { get; set; }
        public int HeaderId { get; set; }
    }
}
