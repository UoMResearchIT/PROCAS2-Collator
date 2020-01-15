using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Models.ServiceBusMessages;

namespace PROCAS2.Services.App
{
    public interface IScreeningService
    {
        bool CreateScreeningRecord(bool useScreeningNumber, string hashedPatientId, ScreeningXlsMessage xlsMessage, int imageId, int densityId, string acquisitionDateTime);
        bool CreateImageRecord(bool useScreeningNumber, string hashedPatientId, string imageFileName, int numImage, out int imageId);
        bool CreateDensityRecord(bool useScreeningNumber, string hashedPatientId, VolparaDensityMessage densityMessage, out int densityId);
        bool ToggleUsingScoreCard(int id);
    }
}
