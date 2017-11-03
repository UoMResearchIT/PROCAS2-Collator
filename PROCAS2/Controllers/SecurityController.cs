using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;



namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    public class SecurityController : Controller
    {
        // GET: Security
        public ActionResult Unauthorised()
        {
            return View("Unauthorised");
        }
    }
}