using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;


namespace PROCAS2.CustomHtmlHelpers
{
    public static class FileUploadHelper
    {
        public static MvcHtmlString FileUpload(this HtmlHelper helper, string name)
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "file");
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("style", "max-width:300px");

            return new MvcHtmlString(builder.ToString(TagRenderMode.SelfClosing));


        }

    }
}
