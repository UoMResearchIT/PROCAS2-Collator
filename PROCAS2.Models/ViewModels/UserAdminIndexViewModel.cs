using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Models.ViewModels
{
    public class UserAdminIndexViewModel
    {
        public UserAdminIndexViewModel()
        {
            AppUsers = new List<AppUser>();
            RegisteredUsers = new List<string>();
        }



        public List<AppUser> AppUsers { get; set; }
        public List<string> RegisteredUsers { get; set; }
    }
}
