﻿using System;
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

        [MaxLength(10)]
        public string PostCode { get; set; }

        [MaxLength(200)]
        public string LetterFrom { get; set; }

        [MaxLength(200)]
        public string LogoFileName { get; set; }

        [MaxLength(200)]
        public string SigFileName { get; set; }

    }
}
