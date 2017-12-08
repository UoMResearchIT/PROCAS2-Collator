using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class Image
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string OrigName { get; set; }

        [MaxLength(200)]
        public string CurrentName { get; set; }

        public int ParticipantID { get; set; }

        public virtual Participant Participant { get; set; }


        public int? ScreeningRecordV1_5_2Id { get; set; }

        public virtual ScreeningRecordV1_5_2 ScreeningRecordV1_5_2 { get; set; }
    }
}
