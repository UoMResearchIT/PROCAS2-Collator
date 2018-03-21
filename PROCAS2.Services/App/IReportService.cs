using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

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
    }
}
