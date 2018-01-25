using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using PROCAS2.Models.ViewModels;

namespace PROCAS2.Services.App
{
    public interface IParticipantService
    {
        bool UploadNewParticipants(UploadNewParticipantsViewModel model, out UploadResultsViewModel outModel, out MemoryStream hashFile);
        void UploadUpdateParticipants(UploadUpdateParticipantsViewModel model, out UploadResultsViewModel outModel);
        void UploadAskRisk(UploadAskRiskViewModel model, out UploadResultsViewModel outModel);
        List<string> UpdateParticipantFromUI(ParticipantEditViewModel model);
        bool DeleteParticipant(string id);
        bool DoesNHSNumberExist(string NHSNumber);
        bool DoesHashedNHSNumberExist(string hash);
        bool SetConsentFlag(string hashedNHSNumber);
        bool CreateRiskLetter(string hashedNHSNumber, string riskScore, string riskCategory, List<string> letterParts);
    }
}
