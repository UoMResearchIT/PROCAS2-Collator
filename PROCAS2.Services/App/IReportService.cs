using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using PROCAS2.Models.ViewModels.Reports;

namespace PROCAS2.Services.App
{
    public interface IReportService
    {
        MemoryStream PatientReport(List<string> NHSNumber);
        MemoryStream Histology();
        MemoryStream YetToConsent();
        MemoryStream YetToGetFull();
        MemoryStream YetToAskForRisk();
        MemoryStream YetToReceiveLetter();
        MemoryStream YetToSendLetter();
        MemoryStream WaitingForVolpara();
        MemoryStream WaitingForVolparaNear6Weeks();
        MemoryStream AskForRiskLetters();
        MemoryStream ScreeningFirstOffered();
        MemoryStream ScreeningWithin180Days();
        MemoryStream NumberTechnicalRecalls();
        MemoryStream NumberAssessmentRecalls();
        MemoryStream NumberRoutineRecalls();
        MemoryStream ChemoDisagreed();
        MemoryStream ChemoNotApp();
        MemoryStream ChemoNotFilled();
        MemoryStream ChemoFilled();
        MemoryStream SubsequentFamilyHistory();
        MemoryStream SubsequentMoreFrequent();
        MemoryStream BreastCancerDiagnoses();
        MemoryStream Consented();
        MemoryStream Invited();
        MemoryStream Volpara();
        MemoryStream QueryDB(QueryDatabaseViewModel model);
    }
}
