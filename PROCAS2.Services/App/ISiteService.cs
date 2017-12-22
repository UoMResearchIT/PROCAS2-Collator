using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Models.ViewModels;

namespace PROCAS2.Services.App
{
    public interface ISiteService
    {
        List<SiteNumber> ReturnAllSiteNumbers();
        SiteEditViewModel FillEditViewModel(string code);
        void SaveSiteRecord(SiteEditViewModel model);
        bool DeleteSite(string code);
    }
}
