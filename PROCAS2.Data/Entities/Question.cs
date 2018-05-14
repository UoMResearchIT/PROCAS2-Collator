using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class Question
    {
        public int Id { get; set; }

        [MaxLength(40)]
        public string Code { get; set; }
        [MaxLength(300)]
        public string Text { get; set; }


        //public virtual ICollection<QuestionAnswer> QuestionAnswers { get; set; }
    }
}
