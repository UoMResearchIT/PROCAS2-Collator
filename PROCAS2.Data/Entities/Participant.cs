﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROCAS2.Data.Entities
{
    public class Participant
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string ScreeningNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        [MaxLength(12)]
        public string NHSNumber { get; set; }

        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateFirstAppointment { get; set; }

        public DateTime DateActualAppointment { get; set; }

        public int BMI { get; set; }


        [MaxLength(200)]
        public string GPName { get; set; }


        public int ScreeningSiteId { get; set; }

        public bool Deceased { get; set; }

        public bool Withdrawn { get; set; }

        public bool SentRisk { get; set; }

        public string RiskLetterContent { get; set; }

        public bool FHCReferral { get; set; }

        public bool Chemoprevention { get; set; }

        public bool Diagnosed { get; set; }

        public bool Consented { get; set; }

        public bool MailingList { get; set; }

        public double RiskScore { get; set; }

        [MaxLength(20)]
        public string RiskCategory { get; set; }
        [ForeignKey("LastEvent")]
        public int LastEventId { get; set; }

        

        public virtual ICollection<ParticipantEvent> ParticipantEvents { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }

        public virtual GeneticRecord GeneticRecord { get; set; }
        public virtual ScreeningSite ScreeningSite { get; set; }
        
        public virtual ParticipantEvent LastEvent { get; set; }
    }
}
