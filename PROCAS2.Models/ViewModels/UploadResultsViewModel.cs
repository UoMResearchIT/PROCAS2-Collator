using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Models.ViewModels
{
    public class UploadResultsViewModel
    {
        public UploadResultsViewModel()
        {
            Messages = new List<string>();
        }


        public List<string> Messages { get; set; }
    }
}
