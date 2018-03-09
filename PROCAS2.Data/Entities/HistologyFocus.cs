using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROCAS2.Data.Entities
{
    public class HistologyFocus
    {

        public int Id { get; set; }
        public int HistologyId { get; set; }

  
        public int FocusNumber { get; set; }

        [ForeignKey("Pathology")]
        public int? PathologyId { get; set; }

        [ForeignKey("Invasive")]
        public int? InvasiveId { get; set; }

        public double? InvasiveTumourSize { get; set; }

        public double? WholeTumourSize { get; set; }

        public int? InvasiveGrade { get; set; }

        [ForeignKey("DCISGrade")]
        public int? DCISGradeId { get; set; }

       
        public int? LymphNodesRemoved { get; set; }

        public int? LymphNodesPositive { get; set; }

        [ForeignKey("VascularInvasion")]
        public int? VascularInvasionId { get; set; }

        public int? ERScore { get; set; }

        public int? PRScore { get; set; }

        [ForeignKey("HER2Score")]
        public int? HER2ScoreId { get; set; }

        public double? KISixtySeven { get; set; }

        [ForeignKey("TNMStage")]
        public int? TNMStageId { get; set; }

        public virtual HistologyLookup Pathology { get; set; }
        public virtual HistologyLookup Invasive { get; set; }
        public virtual HistologyLookup DCISGrade { get; set; }
        public virtual HistologyLookup VascularInvasion { get; set; }
        public virtual HistologyLookup HER2Score { get; set; }
        public virtual HistologyLookup TNMStage { get; set; }

        public virtual Histology Histology { get; set; }
    }
}
