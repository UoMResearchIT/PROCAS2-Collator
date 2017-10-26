using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class Response
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int ParticipantId { get; set; }
        public int AnswerId { get; set; }
        public DateTime ResponseDate { get; set; }
        [MaxLength(200)]
        public string OtherText { get; set; }


        public virtual Question Question { get; set; }
        public virtual QuestionAnswer QuestionAnswer { get; set; }
        public virtual Response Reponse { get; set; }
    }
}
