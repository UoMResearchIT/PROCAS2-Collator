using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Models.ViewModels
{
    public class UserAdminCreateViewModel
    {
        [Required]
        [MaxLength(200)]
        [EmailAddress]
        [Display(Name ="User Name")]
        public string UserCode { get; set; }

        [Required]
        [Display(Name ="Super User")]
        public bool SuperUser { get; set; }

        [Required]
        public bool Active { get; set; }
    }
}
