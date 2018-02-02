using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.IO;

using PROCAS2.Models.ViewModels;

namespace PROCAS2.Services.App
{
    public interface IExportService
    {
        ExportResultsViewModel GenerateLetters(ExportLettersViewModel model);
        ExportResultsViewModel GenerateLetters(string letterId);

        string RenderRazorViewToString(ControllerContext context, object model, string viewName);
        MemoryStream GenerateWordDoc(string html, ExportResultsViewModel model);
        bool ValidateNHSNumberForExport(string NHSNumber, out string errString);
    }
}
