using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class GeneticRecordItem
    {
        public int Id { get; set; }

        [MaxLength(12)]
        public string ItemLabel { get; set; }

        [MaxLength(2)]
        public string GeneticCode { get; set; }

        public double Score { get; set; }

        public int GeneticRecordId { get; set; }

        public virtual GeneticRecord GeneticRecord { get; set; }
    }
}
