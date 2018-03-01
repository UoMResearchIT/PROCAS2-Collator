using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class HistologyLookup
    {
        public int Id { get; set; }

        public string LookupType { get; set; }

        public string LookupDescription { get; set; }
    }
}
