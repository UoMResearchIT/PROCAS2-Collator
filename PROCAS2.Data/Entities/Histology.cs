using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class Histology
    {
        public int ID { get; set; }
        public int ParticipantId { get; set; }
        public DateTime AssessmentDate { get; set; }


        public virtual Participant Participant { get; set; }
    }
}
