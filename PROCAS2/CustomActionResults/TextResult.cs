using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;

namespace PROCAS2.CustomActionResults
{
    public class TextResult : ActionResult
    {
        private MemoryStream _spreadStream;
        private string _fileName;

        public TextResult(MemoryStream spreadsheetStream, string fileName)
        {

            _spreadStream = spreadsheetStream;

            if (String.IsNullOrEmpty(fileName) == true)
                _fileName = "Default.txt";
            else
                _fileName = fileName;
        }

        public override void ExecuteResult(ControllerContext context)
        {

            context.HttpContext.Response.Clear();

            context.HttpContext.Response.AddHeader("Content-Disposition",
                String.Format("attachment;filename={0}", _fileName));
            // String.Format("filename={0}", _fileName));
            //context.HttpContext.Response.CacheControl = "max-age=0";
            context.HttpContext.Response.AddHeader("Pragma", "public");
            //context.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.HttpContext.Response.ContentType = "application/download";
            //context.HttpContext.Response.AppendHeader("Expires", DateTime.Now.AddDays(1).ToShortDateString());

            _spreadStream.WriteTo(context.HttpContext.Response.OutputStream);
            _spreadStream.Close();
            context.HttpContext.Response.End();

        }
    }
}