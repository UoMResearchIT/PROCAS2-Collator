using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace PROCAS2.CustomAttributes
{
    public class RedirectIfNotTestAttribute:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
#if !TESTBUILD
            filterContext.Result = new RedirectResult("/");
            return;
#endif
        }

    }
}