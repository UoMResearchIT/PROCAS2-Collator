using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Models.ViewModels
{
    public class ParticipantListViewModel
    {
        public ParticipantListViewModel()
        {
            Participants = new List<Participant>();
        }

        public List<Participant> Participants { get; set; }
    }
}
