using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Models.ViewModels;

namespace PROCAS2.Services.App
{
    public interface IExportService
    {
        ExportResultsViewModel GenerateLetters(ExportLettersViewModel model);
    }
}
