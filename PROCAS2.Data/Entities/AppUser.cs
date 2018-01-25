using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class AppUser
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string UserCode { get; set; }
        public bool Active { get; set; }
        public bool SuperUser { get; set; }
        public bool SystemUser { get; set; }

        public virtual ICollection<ParticipantEvent> ParticipantEvents { get; set; }

    }
}
