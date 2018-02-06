using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{



    public class ExportLettersViewModel
    {
        [Display(Name ="NHS_NUMBER", ResourceType = typeof(ExportResources))]
        public string NHSNumber { get; set; }

        [Display(Name = "ALL_READY", ResourceType = typeof(ExportResources))]
        public bool AllReady { get; set; }

        [Display(Name ="SITE_TO_PROCESS", ResourceType =typeof(ExportResources))]
        public string SiteToProcess { get; set; }
    }
}
