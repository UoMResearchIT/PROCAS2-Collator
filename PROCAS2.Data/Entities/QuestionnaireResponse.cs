using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Data.Entities
{
    public class QuestionnaireResponse
    {
        public int Id { get; set; }

        public DateTime? QuestionnaireStart { get; set; }
        public DateTime? QuestionnaireEnd { get; set; }

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; }


        public virtual ICollection<QuestionnaireResponseItem> QuestionnaireResponseItems { get; set; }
    }
}
