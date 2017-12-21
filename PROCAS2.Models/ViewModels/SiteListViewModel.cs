using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Models.ViewModels
{
    public class SiteListViewModel
    {
        public SiteListViewModel()
        {
            ScreeningSites = new List<ScreeningSite>();
            SiteNumbers = new List<SiteNumber>();
        }


        public List<ScreeningSite> ScreeningSites { get; set; } 
        public List<SiteNumber> SiteNumbers { get; set; }
    }


    public class SiteNumber
    {
        public string SiteCode { get; set; }
        public int NumberOfParticipants { get; set; }
    }
}
