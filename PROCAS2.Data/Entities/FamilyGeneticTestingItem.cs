using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    /// <summary>
    /// For self-reported genetic tests on family members
    /// </summary>
    public class FamilyGeneticTestingItem
    {
        public int Id { get; set; }

        public int QuestionnaireResponseId { get; set; }

        [MaxLength(20)]
        public string RelationshipCode { get; set; }

        [MaxLength(40)]
        public string RelationshipDescription { get; set; }

        [MaxLength(3)]
        public string RelationshipIdentifier { get; set; }

        [MaxLength(1)]
        public string Gender { get; set; }

        public int? Age { get; set; }

        [MaxLength(10)]
        public string GeneticTest { get; set; }

        [MaxLength(20)]
        public string TestSignificance { get; set; }

        public virtual QuestionnaireResponse QuestionnaireResponse { get; set; }
    }
}
