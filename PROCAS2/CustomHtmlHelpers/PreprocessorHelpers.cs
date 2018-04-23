using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

namespace PROCAS2.CustomHtmlHelpers
{
    public static class PreprocessorHelpers
    {

        public static bool IsTestBuild(this HtmlHelper htmlHelper)
        {
#if TESTBUILD
            return true;
#else
            return false;
#endif
        }
    }
}