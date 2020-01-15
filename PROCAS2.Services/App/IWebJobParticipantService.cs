using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Services.App
{
    public interface IWebJobParticipantService
    {
        AppUser GetSystemUser(string userId);
        bool DoesHashedNHSNumberExist(string hash);
        bool DoesHashedScreeningNumberExist(string hash);
        bool SetConsentFlag(string hashedNHSNumber);
        bool CreateRiskLetter(string hashedNHSNumber, string riskScore, string riskCategory, string geneticTesting, List<string> letterParts, List<string>letterGPParts);
        string GetStudyNumber(bool useScreeningNumber, string hashedNHSNumber);
        bool AddEvent(Participant participant, AppUser user, DateTime eventDate, string eventCode, string eventNotes, string reason = null);
        bool SetBMI(string hashedNHSNumber, string answerText);
        string GetHashedPatientId(string hashedPatientID);
    }
}
