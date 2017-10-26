using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class QuestionAnswer
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Text { get; set; }
        [MaxLength(50)]
        public string Code { get; set; }
        public int Order { get; set; }

        public int QuestionId { get; set; }


        public virtual Question Question { get; set; }
    }
}
