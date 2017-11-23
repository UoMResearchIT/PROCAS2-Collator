using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

using PROCAS2.Data.Entities;
using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class ParticipantDetailsViewModel
    {
        public void Hydrate()
        {
            
        }

        public Participant Participant { get; set; }

        /// <summary>
        /// Formatting the name.
        /// </summary>
        [Display(Name = "PARTICIPANT_NAME", ResourceType = typeof(ParticipantResources))]
        public string Name {
            get
            {
                if (Participant != null)
                {
                    return Participant.Title + " " + Participant.FirstName + " " + Participant.LastName;
                }
                else
                    return null;
            }
        }
    }
}
