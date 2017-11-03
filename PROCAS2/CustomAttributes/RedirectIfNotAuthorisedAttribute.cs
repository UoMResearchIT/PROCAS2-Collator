using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace PROCAS2.CustomAttributes
{
    public class RedirectIfNotAuthorisedAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectResult(PrependSchemeAndAuthority("Security/Unauthorised/"));

            }
        }

        public string PrependSchemeAndAuthority(string url)
        {
            try
            {
                if (HttpContext.Current.Request.Url.Authority.Contains("localhost"))
                {
                    return HttpContext.Current.Request.Url.Scheme + "://"
                            + HttpContext.Current.Request.Url.Authority + "/"
                            + url;
                }
                string urlBase = ConfigurationManager.AppSettings["UrlBase"];
                if (urlBase != null)
                {
                    return urlBase + "/" + url;
                }
                else
                {
                    return HttpContext.Current.Request.Url.Scheme + "://"
                        + HttpContext.Current.Request.Url.Authority + "/"
                        + url;
                }
            }
            catch
            {
                return url;
            }
        }

    }
}