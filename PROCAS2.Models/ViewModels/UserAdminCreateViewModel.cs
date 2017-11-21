using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class UserAdminCreateViewModel
    {
        [Required]
        [MaxLength(200)]
        [EmailAddress]
        [Display(Name = "VM_USER_ADMIN_CREATE_USERNAME", ResourceType = typeof(PROCASRes))]
        public string UserCode { get; set; }

        [Required]
        [Display(Name = "VM_USER_ADMIN_CREATE_SUPERUSER",  ResourceType = typeof(PROCASRes))]
        public bool SuperUser { get; set; }

        [Required]
        [Display(Name = "VM_USER_ADMIN_CREATE_ACTIVE", ResourceType = typeof(PROCASRes))]
        public bool Active { get; set; }
    }
}
