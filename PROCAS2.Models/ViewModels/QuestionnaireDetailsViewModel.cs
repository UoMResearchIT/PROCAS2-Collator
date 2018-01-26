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
    public class QuestionnaireDetailsViewModel
    {
        [Display(Name = "NHS_NUMBER", ResourceType = typeof(ParticipantResources))]
        public string NHSNumber{ get; set; }

        [Display(Name = "QUESTIONNAIRE_START", ResourceType = typeof(ParticipantResources))]
        public DateTime? QuestionnaireStart { get; set; }

        [Display(Name = "QUESTIONNAIRE_END", ResourceType = typeof(ParticipantResources))]
        public DateTime? QuestionnaireEnd { get; set; }

        public List<QuestionnaireResponseItem> ResponseItems { get; set; }
        public List<FamilyHistoryItem> HistoryItems { get; set; }

    }
}
