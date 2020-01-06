using System;
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

        [MaxLength(400)]
        public string HashedNHSNumber { get; set; }

        [MaxLength(20)]
        public string ScreeningNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string NHSNumber { get; set; }

        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateFirstAppointment { get; set; }

        public DateTime? DateActualAppointment { get; set; }

        public double? BMI { get; set; }


        [MaxLength(200)]
        public string GPName { get; set; }


        public int? ScreeningSiteId { get; set; }

        public bool Deceased { get; set; }

        public bool Withdrawn { get; set; }

        public bool SentRisk { get; set; }

        public bool AttendedScreening { get; set; } // attended screening within 6 weeks

        public bool FHCReferral { get; set; }

        public bool MoreFrequentScreening { get; set; }

        public bool Chemoprevention { get; set; }
        public bool ChemoAgreedInClinic { get; set; }
        [ForeignKey("ChemoPreventionDetails")]
        public int? ChemoPreventionDetailsId { get; set; }

        public bool Diagnosed { get; set; }

        public bool Consented { get; set; }

        public DateTime? DateConsented { get; set; }

        public bool MailingList { get; set; }

        public bool Deleted { get; set; }

        public bool AskForRiskLetter { get; set; }

      
        public int StudyNumber { get; set; } // Used for searching in CRA Health

        [ForeignKey("InitialScreeningOutcome")]
        public int? InitialScreeningOutcomeId { get; set; }

        [ForeignKey("FinalTechnicalOutcome")]
        public int? FinalTechnicalOutcomeId { get; set; }

        [ForeignKey("FinalAssessmentOutcome")]
        public int? FinalAssessmentOutcomeId { get; set; }

        public bool RiskConsultationEligible { get; set; }

        public bool RiskConsultationBooked { get; set; }

        public bool RiskConsultationCompleted { get; set; }

        [ForeignKey("RiskConsultationType")]
        public int? RiskConsultationTypeId { get; set; }

        public bool RiskConsultationLetterSent { get; set; }

        [MaxLength(5000)]
        public string RiskConsultationComments { get; set; }

        [ForeignKey("LastEvent")]
        public int? LastEventId { get; set; }

        [MaxLength(400)]
        public string HashedScreeningNumber { get; set; }

        public bool UseScreeningNumber { get; set; }

        public virtual ICollection<RiskLetter> RiskLetters { get; set; }
        public virtual ICollection<ParticipantEvent> ParticipantEvents { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<ScreeningRecordV1_5_4> ScreeningRecordV1_5_4s { get; set; }
        public virtual ICollection<VolparaDensity> VolparaDensities { get; set; }
        public virtual ICollection<QuestionnaireResponse> QuestionnaireResponses { get; set; }
        public virtual ICollection<Histology> Histologies { get; set; }

        public virtual ParticipantLookup InitialScreeningOutcome { get; set; }
        public virtual ParticipantLookup FinalTechnicalOutcome { get; set; }
        public virtual ParticipantLookup FinalAssessmentOutcome { get; set; }
        public virtual ParticipantLookup ChemoPreventionDetails { get; set; }
        public virtual ParticipantLookup RiskConsultationType { get; set; }

        public virtual GeneticRecord GeneticRecord { get; set; }
        public virtual ScreeningSite ScreeningSite { get; set; }
        
        public virtual ParticipantEvent LastEvent { get; set; }
    }
}
