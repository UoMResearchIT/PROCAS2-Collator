using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Services.Utility
{
    public interface IPROCAS2UserManager
    {
        bool CheckUserRecord(string userName);
        List<AppUser> GetAllAppUsers();
        void Suspend(int userId, bool flag);
        void SuperUser(int userid, bool flag);
        bool IsSuperUser(string userName);
    }
}
