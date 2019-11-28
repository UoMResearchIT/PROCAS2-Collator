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

        [MaxLength(20)]
        [Required]
        [Display(Name = "SITE_TELEPHONE", ResourceType = typeof(SiteResources))]
        public string Telephone { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        [Display(Name = "SITE_EMAILADDRESS", ResourceType = typeof(SiteResources))]
        public string EmailAddress { get; set; }

        [MaxLength(200)]
        [Required]
        [Display(Name = "SITE_NAMEFROM", ResourceType = typeof(SiteResources))]
        public string LetterFrom { get; set; }

        [MaxLength(30)]
        [Required]
        [Display(Name = "SITE_SIGNATURE", ResourceType = typeof(SiteResources))]
        public string Signature { get; set; }

        [MaxLength(20)]
        [Required]
        [Display(Name="SITE_TRUSTCODE", ResourceType= typeof(SiteResources))]
        public string TrustCode { get; set; }

        [MaxLength(30)]
        [Required]
        [Display(Name = "SITE_IMAGEHEADERRIGHT", ResourceType = typeof(SiteResources))]
        public string LogoHeaderRight { get; set; }

        [Required]
        [Display(Name = "SITE_IMAGEHEADERRIGHT_HEIGHT", ResourceType = typeof(SiteResources))]
        public double LogoHeaderRightHeight { get; set; }

        [Required]
        [Display(Name = "SITE_IMAGEHEADERRIGHT_WIDTH", ResourceType = typeof(SiteResources))]
        public double LogoHeaderRightWidth { get; set; }


        [MaxLength(30)]
        [Display(Name = "SITE_IMAGEFOOTERRIGHT", ResourceType = typeof(SiteResources))]
        public string LogoFooterRight { get; set; }

        [Required]
        [Display(Name = "SITE_IMAGEFOOTERRIGHT_HEIGHT", ResourceType = typeof(SiteResources))]
        public double LogoFooterRightHeight { get; set; }

        [Required]
        [Display(Name = "SITE_IMAGEFOOTERRIGHT_WIDTH", ResourceType = typeof(SiteResources))]
        public double LogoFooterRightWidth { get; set; }

        [MaxLength(30)]
        [Display(Name = "SITE_IMAGEFOOTERLEFT", ResourceType = typeof(SiteResources))]
        public string LogoFooterLeft { get; set; }

        [Required]
        [Display(Name = "SITE_IMAGEFOOTERLEFT_HEIGHT", ResourceType = typeof(SiteResources))]
        public double LogoFooterLeftHeight { get; set; }

        [Required]
        [Display(Name = "SITE_IMAGEFOOTERLEFT_WIDTH", ResourceType = typeof(SiteResources))]
        public double LogoFooterLeftWidth { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "SITE_FAMILY_HISTORY_CLINIC", ResourceType = typeof(SiteResources))]
        public string FamilyHistoryClinic { get; set; }

    }
}
