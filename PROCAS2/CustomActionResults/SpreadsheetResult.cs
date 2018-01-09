using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using System.IO;

namespace PROCAS2.CustomActionResults
{
    public class SpreadsheetResult:ActionResult
    {
        private string _fileName;
        private MemoryStream _mStream;

        public SpreadsheetResult(MemoryStream mStream, string fileName)
        {

            _fileName = fileName;
            _mStream = mStream;

        }


        /// <summary>
        /// Output the Memorystream in OpenXML Document format.
        /// </summary>
        /// <param name="context"></param>
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.HttpContext.Response.AppendHeader("Content-Disposition", String.Concat("attachment;filename=\"", _fileName, ".xlsx\""));

            context.HttpContext.Response.AddHeader("Content-Length", _mStream.Length.ToString());
            _mStream.WriteTo(context.HttpContext.Response.OutputStream);
            _mStream.Close();


            context.HttpContext.Response.End();

        }
    }
}