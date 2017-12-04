using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace PROCAS2.CustomActionResults
{
    public class WordResult:ActionResult
    {

        private object _model;
        private string _viewName;
        private string _fileName;
        private MemoryStream _mStream;

        public WordResult(MemoryStream mStream, string fileName)
        {
           // _model = model;
            _fileName = fileName;
            _mStream = mStream;
            //_viewName = viewName;
        }

     

        //private string RenderRazorViewToString(ControllerContext context)
        //{
        //    context.Controller.ViewData.Model = _model;
        //    using (var sw = new StringWriter())
        //    {
        //        var viewResult = ViewEngines.Engines.FindPartialView(context,
        //                                                                 _viewName);
        //        var viewContext = new ViewContext(context, viewResult.View,
        //                                     context.Controller.ViewData, context.Controller.TempData, sw);
        //        viewResult.View.Render(viewContext, sw);
        //        viewResult.ViewEngine.ReleaseView(context, viewResult.View);
        //        return sw.GetStringBuilder().ToString();
        //    }
        //}


        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            context.HttpContext.Response.AppendHeader("Content-Disposition", String.Concat("attachment;filename=\"", _fileName, ".docx\""));

            context.HttpContext.Response.AddHeader("Content-Length", _mStream.Length.ToString());
            _mStream.WriteTo(context.HttpContext.Response.OutputStream);
            _mStream.Close();
            //string htmlString = this.RenderRazorViewToString(context);
            //context.HttpContext.Response.AppendHeader("Content-Disposition", string.Format("filename={0}.doc", _fileName));
            //context.HttpContext.Response.ContentType = "application/msword";
            ////application/vnd.openxmlformats-officedocument.wordprocessingml.document
            //context.HttpContext.Response.Write(htmlString);

            context.HttpContext.Response.End();

        }
    }
}