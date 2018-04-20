using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;

using PROCAS2.Data.Entities;
using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class ParticipantEditViewModel
    {
        public ParticipantEditViewModel()
        {
            PropertyChanged += ViewModel_PropertyChanged;

            ScreeningSites = new List<ScreeningSite>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Event called when the participant property is changed. Hydrate the other propertys.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ParticipantEditViewModel model = sender as ParticipantEditViewModel;
            Title = model.Participant.Title;
            FirstName = model.Participant.FirstName;
            LastName = model.Participant.LastName;
            NHSNumber = model.Participant.NHSNumber;
            ScreeningNumber = model.Participant.ScreeningNumber;
            DOB = model.Participant.DateOfBirth;
            DOFA = model.Participant.DateFirstAppointment;
            DOAA = model.Participant.DateActualAppointment;
            DateConsented = model.Participant.DateConsented;
            ScreeningSite = model.Participant.ScreeningSite.Name;
            BMI = model.Participant.BMI.ToString();
            SentRisk = model.Participant.SentRisk;
            FHCReferral = model.Participant.FHCReferral;
            MoreFrequentScreening = model.Participant.MoreFrequentScreening;
            Chemo = model.Participant.Chemoprevention;
            GPName = model.Participant.GPName;
            Consented = model.Participant.Consented;
            Deceased = model.Participant.Deceased;
            Withdrawn = model.Participant.Withdrawn;
            Diagnosed = model.Participant.Diagnosed;
            MailingList = model.Participant.MailingList;
            AttendedScreening = model.Participant.AttendedScreening;
            AskForRiskLetter = model.Participant.AskForRiskLetter;
            ChemoAgreedInClinic = model.Participant.ChemoAgreedInClinic;
            StudyNumber = model.Participant.StudyNumber;
            RiskConsultationLetterSent = model.Participant.RiskConsultationLetterSent;
            RiskConsultationEligible = model.Participant.RiskConsultationEligible;
            RiskConsultationCompleted = model.Participant.RiskConsultationCompleted;
            RiskConsultationComments = model.Participant.RiskConsultationComments;
            RiskConsultationBooked = model.Participant.RiskConsultationBooked;

            if (model.Participant.ChemoPreventionDetails == null)
                ChemoPreventionDetailsId = null;
            else
                ChemoPreventionDetailsId = model.Participant.ChemoPreventionDetails.Id;

            if (model.Participant.InitialScreeningOutcome == null)
                InitialScreeningOutcomeId = null;
            else
                InitialScreeningOutcomeId = model.Participant.InitialScreeningOutcome.Id;

            if (model.Participant.FinalTechnicalOutcome == null)
                FinalTechnicalOutcomeId = null;
            else
                FinalTechnicalOutcomeId = model.Participant.FinalTechnicalOutcome.Id;

            if (model.Participant.FinalAssessmentOutcome == null)
                FinalAssessmentOutcomeId = null;
            else
                FinalAssessmentOutcomeId = model.Participant.FinalAssessmentOutcome.Id;

            if (model.Participant.RiskConsultationType == null)
                RiskConsultationTypeId = null;
            else
                RiskConsultationTypeId = model.Participant.RiskConsultationType.Id;


            Address homeAddress = model.Participant.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault();
            if (homeAddress != null)
            {
                HomeAddress1 = homeAddress.AddressLine1;
                HomeAddress2 = homeAddress.AddressLine2;
                HomeAddress3 = homeAddress.AddressLine3;
                HomeAddress4 = homeAddress.AddressLine4;
                HomePostCode = homeAddress.PostCode;
                HomeEmail = homeAddress.EmailAddress;
            }

            Address gpAddress = model.Participant.Addresses.Where(x => x.AddressType.Name == "GP").FirstOrDefault();
            if (gpAddress != null)
            {
                GPAddress1 = gpAddress.AddressLine1;
                GPAddress2 = gpAddress.AddressLine2;
                GPAddress3 = gpAddress.AddressLine3;
                GPAddress4 = gpAddress.AddressLine4;
                GPPostCode = gpAddress.PostCode;
                GPEmail = gpAddress.EmailAddress;
            }

        }

        private Participant _participant;
        public Participant Participant
        {
            get
            {
                return _participant;
            }
            set
            {
                if (_participant != value)
                {
                    _participant = value;
                    NotifyPropertyChanged();
                }
            }
        }


        [Display(Name = "TITLE", ResourceType = typeof(ParticipantResources))]
        [MaxLength(50)]
        public string Title { get; set; }

        [Display(Name = "FIRST_NAME", ResourceType = typeof(ParticipantResources))]
        [MaxLength(50)]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "LAST_NAME", ResourceType = typeof(ParticipantResources))]
        [MaxLength(50)]
        [Required]
        public string LastName { get; set; }


        [Display(Name = "ADDRESS", ResourceType = typeof(ParticipantResources))]
        [MaxLength(200)]
        [Required]
        public string HomeAddress1 { get; set; }

        [MaxLength(200)]
        public string HomeAddress2 { get; set; }

        [MaxLength(200)]
        public string HomeAddress3 { get; set; }

        [MaxLength(200)]
        public string HomeAddress4 { get; set; }

        [Display(Name = "POST_CODE", ResourceType = typeof(ParticipantResources))]
        [MaxLength(10)]
        [Required]
        public string HomePostCode { get; set; }

        [Display(Name = "HOMEEMAIL", ResourceType = typeof(ParticipantResources))]
        [MaxLength(200)]
        [EmailAddress]
        public string HomeEmail { get; set; }

        [Display(Name = "GP_NAME", ResourceType = typeof(ParticipantResources))]
        [MaxLength(200)]
        [Required]
        public string GPName { get; set; }

        [Display(Name = "ADDRESS", ResourceType = typeof(ParticipantResources))]
        [MaxLength(200)]
        [Required]
        public string GPAddress1 { get; set; }

        [MaxLength(200)]
        public string GPAddress2 { get; set; }

        [MaxLength(200)]
        public string GPAddress3 { get; set; }

        [MaxLength(200)]
        public string GPAddress4 { get; set; }

        [Display(Name = "POST_CODE", ResourceType = typeof(ParticipantResources))]
        [MaxLength(10)]
        [Required]
        public string GPPostCode { get; set; }

        [Display(Name = "GP_EMAIL", ResourceType = typeof(ParticipantResources))]
        [MaxLength(200)]
        [EmailAddress]
        public string GPEmail { get; set; }

        public string NHSNumber { get; set; }

        [Display(Name = "SCREENING_NUMBER", ResourceType = typeof(ParticipantResources))]
        [MaxLength(20)]
        [Required]
        public string ScreeningNumber { get; set; }

        [Required]
        [Display(Name = "DOB", ResourceType = typeof(ParticipantResources))]
        public DateTime? DOB { get; set; }

        
        [Display(Name = "DOFA", ResourceType = typeof(ParticipantResources))]
        [Required]
        public DateTime? DOFA { get; set; }

        [Display(Name = "SCREENING_SITE", ResourceType = typeof(ParticipantResources))]
        [Required]
        public string ScreeningSite { get; set; }

        [Display(Name = "DOAA", ResourceType = typeof(ParticipantResources))]
        [Required]
        public DateTime? DOAA { get; set; }

        [Display(Name = "BMI", ResourceType = typeof(ParticipantResources))]
        [Range(0, 40)]
        public string BMI { get; set; }

        [Display(Name = "CHEMO", ResourceType = typeof(ParticipantResources))]
        public bool Chemo { get; set; }
        [Display(Name = "SENT_RISK", ResourceType = typeof(ParticipantResources))]
        public bool SentRisk { get; set; }
        [Display(Name = "FHC_REFERRAL", ResourceType = typeof(ParticipantResources))]
        public bool FHCReferral { get; set; }

        [Display(Name = "MORE_FREQUENT", ResourceType = typeof(ParticipantResources))]
        public bool MoreFrequentScreening { get; set; }

        [Display(Name = "CONSENTED", ResourceType = typeof(ParticipantResources))]
        public bool Consented { get; set; }
        [Display(Name = "DIAGNOSED", ResourceType = typeof(ParticipantResources))]
        public bool Diagnosed { get; set; }
        [Display(Name = "DECEASED", ResourceType = typeof(ParticipantResources))]
        public bool Deceased { get; set; }
        [Display(Name = "WITHDRAWN", ResourceType = typeof(ParticipantResources))]
        public bool Withdrawn { get; set; }
        [Display(Name = "MAILING_LIST", ResourceType = typeof(ParticipantResources))]
        public bool MailingList { get; set; }
        [Display(Name = "ATTENDED_SCREENING", ResourceType = typeof(ParticipantResources))]
        public bool AttendedScreening { get; set; }

        [Display(Name = "ASKFORRISK", ResourceType = typeof(ParticipantResources))]
        public bool AskForRiskLetter { get; set; }

        [Display(Name = "DATE_CONSENTED", ResourceType = typeof(ParticipantResources))]
        [Required]
        public DateTime? DateConsented { get; set; }

        [Display(Name = "CHEMO_AGREED", ResourceType = typeof(ParticipantResources))]
        public bool ChemoAgreedInClinic { get; set; }

        [Display(Name = "CHEMO_DETAILS", ResourceType = typeof(ParticipantResources))]
        public int? ChemoPreventionDetailsId { get; set; }

        [Display(Name = "INITIAL_SCREENING", ResourceType = typeof(ParticipantResources))]
        public int? InitialScreeningOutcomeId { get; set; }

        [Display(Name = "FINAL_TECHNICAL", ResourceType = typeof(ParticipantResources))]
        public int? FinalTechnicalOutcomeId { get; set; }


        [Display(Name = "FINAL_ASSESSMENT", ResourceType = typeof(ParticipantResources))]
        public int? FinalAssessmentOutcomeId { get; set; }

        [Display(Name = "STUDY_NUMBER", ResourceType = typeof(ParticipantResources))]
        public int StudyNumber { get; set; }

        [Display(Name = "RISK_CONS_ELIGIBLE", ResourceType = typeof(ParticipantResources))]
        public bool RiskConsultationEligible { get; set; }

        [Display(Name = "RISK_CONS_BOOKED", ResourceType = typeof(ParticipantResources))]
        public bool RiskConsultationBooked { get; set; }

        [Display(Name = "RISK_CONS_COMPLETED", ResourceType = typeof(ParticipantResources))]
        public bool RiskConsultationCompleted { get; set; }

        [Display(Name = "RISK_CONS_TYPE", ResourceType = typeof(ParticipantResources))]
        public int? RiskConsultationTypeId { get; set; }

        [Display(Name = "RISK_CONS_LETTER_SENT", ResourceType = typeof(ParticipantResources))]
        public bool RiskConsultationLetterSent { get; set; }

        [MaxLength(5000)]
        [Display(Name = "RISK_CONS_COMMENT", ResourceType = typeof(ParticipantResources))]
        public string RiskConsultationComments { get; set; }


        public string Reason { get; set; }

        public List<ScreeningSite> ScreeningSites { get; set; }

        public List<ParticipantLookup> ChemoPreventionDetails { get; set; }
        public List<ParticipantLookup> InitialScreeningOutcome { get; set; }
        public List<ParticipantLookup> FinalTechnicalScreeningOutcome { get; set; }
        public List<ParticipantLookup> FinalAssessmentScreeningOutcome { get; set; }
        public List<ParticipantLookup> RiskConsultationTypes { get; set; }
    }

}
