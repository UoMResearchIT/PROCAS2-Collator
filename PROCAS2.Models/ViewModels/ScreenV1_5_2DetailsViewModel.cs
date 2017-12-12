using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Models.ViewModels
{
    public class ScreenV1_5_2DetailsViewModel
    {
        public ScreenV1_5_2DetailsViewModel()
        {
            ScreeningRecords = new List<ScreeningRecordV1_5_2>();
        } 

        public string NHSNumber { get; set; }
        public List<ScreeningRecordV1_5_2> ScreeningRecords { get; set; }
    }
}
