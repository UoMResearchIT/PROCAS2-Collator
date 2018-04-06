using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class ParticipantLookup
    {
        public int Id { get; set; }

        [MaxLength(10)]
        public string LookupType { get; set; }

        [MaxLength(20)]
        public string LookupCode { get; set; }

        [MaxLength(50)]
        public string LookupDescription { get; set; }
    }
}
