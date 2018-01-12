using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Models.ViewModels
{
    public class DashboardAppNewsViewModel
    {
        public DashboardAppNewsViewModel()
        {
            NewsItems = new List<AppNewsItem>();
        }


        public List<AppNewsItem> NewsItems { get; set; }
    }
}
