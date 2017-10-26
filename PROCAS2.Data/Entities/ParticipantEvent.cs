using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class ParticipantEvent
    {
        public int Id { get; set; }

        public int ParticipantId { get; set; }

        public DateTime EventDate { get; set; }

        public int EventTypeId { get; set; }

        [MaxLength(200)]
        public string Notes { get; set; }

        public int AppUserId { get; set; }


        public virtual Participant Participant { get; set; }
        public virtual EventType EventType { get; set; }
        public virtual AppUser AppUser { get; set; }
    }
}
