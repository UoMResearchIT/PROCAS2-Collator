using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class QuestionnaireResponseItem
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int QuestionnaireResponseId { get; set; }
       
       
        [MaxLength(200)]
        public string ResponseText { get; set; }


        public virtual Question Question { get; set; }
        
        public virtual QuestionnaireResponse QuestionnaireResponse { get; set; }
    }
}
