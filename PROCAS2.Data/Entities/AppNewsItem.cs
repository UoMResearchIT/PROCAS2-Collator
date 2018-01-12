using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class AppNewsItem
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Message { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime HighlightUntil { get; set; }
    }
}
