using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Models.ViewModels
{
    public class ParticipantHistoryQueryViewModel
    {
        [Required]
        [MaxLength(10)]
        [Display(Name = "NHS Number")]
        public string NHSNumber { get; set; }
    }
}
