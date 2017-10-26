using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class Image
    {
        public int ID { get; set; }

        [MaxLength(200)]
        public string OrigName { get; set; }

        [MaxLength(200)]
        public string CurrentName { get; set; }

        public int ParticipantID { get; set; }

        public virtual Participant Participant { get; set; }
    }
}
