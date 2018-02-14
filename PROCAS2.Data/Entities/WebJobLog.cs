using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class WebJobLog
    {
        public int Id { get; set; }

        public DateTime LogDate { get; set; }

        public int MessageType { get; set; }

        public int LogLevel { get; set; }

        [MaxLength(500)]
        public string Message { get; set; }

        public string StackTrace { get; set; }

        public string MessageBody { get; set; }

        public bool Reviewed { get; set; }
    }
}
