using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Models.TestModels
{
    public class ReceiveLetterViewModel
    {
        [Required]
        [MaxLength(10)]
        [Display(Name = "NHS Number")]
        public string NHSNumber { get; set; }
    }
}
