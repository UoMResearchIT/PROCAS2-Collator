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
        bool SetConsentFlag(string hashedNHSNumber);
        bool CreateRiskLetter(string hashedNHSNumber, string riskScore, string riskCategory, List<string> letterParts);
        string GetStudyNumber(string hashedNHSNumber);
    }
}
