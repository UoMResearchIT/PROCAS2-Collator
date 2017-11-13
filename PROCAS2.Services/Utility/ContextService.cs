using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;




namespace PROCAS2.Services.Utility
{
    public class ContextService : IContextService
    {

       
        
       

        public string CurrentUserName()
        {
            return HttpContext.Current.User.Identity.Name;
        }

        
    }
}
