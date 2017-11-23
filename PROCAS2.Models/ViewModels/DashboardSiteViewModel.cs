using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Models.ViewModels
{
    public class DashboardSiteViewModel
    {
        public DashboardSiteViewModel()
        {
            Sites = new List<ScreeningSite>();
        }

        public List<ScreeningSite> Sites { get; set; }
    }
}
