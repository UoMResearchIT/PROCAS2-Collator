using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class GeneticRecord
    {
        
        public int Id { get; set; }
        public DateTime DateProvided { get; set; }
        public int SampleNo { get; set; }

        [MaxLength(20)]
        public string SampleIdentifier { get; set; }

        [MaxLength(3)]
        public string NineSixWell { get; set; }
        [MaxLength(3)]
        public string ThreeEightSix { get; set; }

        public double SumScore { get; set; }

       
       
        

        public virtual ICollection<GeneticRecordItem> GeneticRecordItems {get;set;}
    }
}
