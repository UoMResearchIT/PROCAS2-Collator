using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class FamilyHistoryItem
    {
        public int Id { get; set; }

        public int QuestionnaireResponseId { get; set; }

        [MaxLength(20)]
        public string RelationshipCode { get; set; }
      
        [MaxLength(40)]
        public string RelationshipDescription { get; set; }

        [MaxLength(1)]
        public string Gender { get; set; }

        public int? Age { get; set; }

        [MaxLength(100)]
        public string Disease { get; set; }

        public int? AgeOfDiagnosis { get; set; }

        public virtual QuestionnaireResponse QuestionnaireResponse { get; set; }
    }
}
