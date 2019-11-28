using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class ScreeningSite
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(10)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string AddressLine1 { get; set; }

        [MaxLength(200)]
        public string AddressLine2 { get; set; }

        [MaxLength(200)]
        public string AddressLine3 { get; set; }

        [MaxLength(200)]
        public string AddressLine4 { get; set; }

        [MaxLength(30)]
        public string Telephone { get; set; }

        [MaxLength(10)]
        public string PostCode { get; set; }

        [MaxLength(200)]
        public string LetterFrom { get; set; }

        [MaxLength(30)]
        public string LogoHeaderRight { get; set; }

        [MaxLength(30)]
        public string LogoHeaderRightHeight { get; set; }

        [MaxLength(30)]
        public string LogoHeaderRightWidth { get; set; }

        [MaxLength(30)]
        public string LogoFooterLeft { get; set; }

        [MaxLength(30)]
        public string LogoFooterLeftHeight { get; set; }

        [MaxLength(30)]
        public string LogoFooterLeftWidth { get; set; }

        [MaxLength(30)]
        public string LogoFooterRight { get; set; }

        [MaxLength(30)]
        public string LogoFooterRightHeight { get; set; }

        [MaxLength(30)]
        public string LogoFooterRightWidth { get; set; }

        [MaxLength(30)]
        public string Signature { get; set; }

        [MaxLength(20)]
        public string TrustCode { get; set; }

        [MaxLength(200)]
        public string FamilyHealthClinic { get; set; }

        [MaxLength(200)]
        public string EmailAddress { get; set; }

    }
}
