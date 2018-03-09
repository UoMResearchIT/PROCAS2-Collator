using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROCAS2.Data.Entities
{
    public class Histology
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }


        public int PrimaryNumber { get; set; }

        [ForeignKey("DiagnosisType")]
        public int? DiagnosisTypeId { get; set; }

        public DateTime? DiagnosisDate { get; set; }

        public DateTime? MammogramDate { get; set; }

        [ForeignKey("DiagnosisSide")]
        public int? DiagnosisSideId { get; set; }

        public bool DiagnosisMultiFocal { get; set; }

        public string Comments { get; set; }

        public virtual HistologyLookup DiagnosisType { get; set; }
        public virtual HistologyLookup DiagnosisSide { get; set; }

        public virtual Participant Participant { get; set; }

        public virtual ICollection<HistologyFocus> HistologyFoci { get; set; }
    }
}
