using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using PROCAS2.Data.Entities;

namespace PROCAS2.Models.ViewModels
{
    public class ParticipantHistoryDetailsViewModel
    {
        public string NHSNumber { get; set; }

        public int StudyNumber { get; set; }

        public List<ParticipantEvent> Events { get; set; }
    }
}
