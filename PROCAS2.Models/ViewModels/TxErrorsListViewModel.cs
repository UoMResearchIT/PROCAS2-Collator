using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Models.ViewModels
{
    public class TxErrorsListViewModel
    {
        public TxErrorsListViewModel()
        {
            TxErrors = new List<WebJobLog>();
        }

        public List<WebJobLog> TxErrors { get; set; }
    }
}
