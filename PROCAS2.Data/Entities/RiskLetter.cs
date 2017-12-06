using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class RiskLetter
    {
        public int Id { get; set; }

        public DateTime DateReceived { get; set; }

        public DateTime? DateSent { get; set; }

        public double RiskScore { get; set; }

        public string RiskLetterContent { get; set; }

        [MaxLength(20)]
        public string RiskCategory { get; set; }

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; }

    }
}
