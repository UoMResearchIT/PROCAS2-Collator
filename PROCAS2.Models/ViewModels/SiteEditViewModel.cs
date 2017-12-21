using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class SiteEditViewModel
    {
        [MaxLength(200)]
        [Required]
        [Display(Name = "SITE_NAME", ResourceType = typeof(SiteResources))]
        public string Name { get; set; }

        [MaxLength(10)]
        [Required]
        [Display(Name = "SITE_CODE", ResourceType = typeof(SiteResources))]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        [Display(Name = "SITE_ADDRESS", ResourceType = typeof(SiteResources))]
        public string AddressLine1 { get; set; }

        [MaxLength(200)]
        public string AddressLine2 { get; set; }

        [MaxLength(200)]
        public string AddressLine3 { get; set; }

        [MaxLength(200)]
        public string AddressLine4 { get; set; }

        [MaxLength(10)]
        [Required]
        [Display(Name = "SITE_POSTCODE", ResourceType = typeof(SiteResources))]
        public string PostCode { get; set; }

       

        [MaxLength(200)]
        [Required]
        [Display(Name = "SITE_LOGOFILENAME", ResourceType = typeof(SiteResources))]
        public string LogoFileName { get; set; }

        [MaxLength(200)]
        [Required]
        [Display(Name = "SITE_NAMEFROM", ResourceType = typeof(SiteResources))]
        public string LetterFrom { get; set; }

        [MaxLength(200)]
        [Required]
        [Display(Name = "SITE_SIGFILENAME", ResourceType = typeof(SiteResources))]
        public string SigFileName { get; set; }

    }
}
