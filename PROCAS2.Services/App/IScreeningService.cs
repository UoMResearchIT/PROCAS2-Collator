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
        bool CreateScreeningRecord(string hashedPatientId, ScreeningXlsMessage xlsMessage, int imageId, int densityId);
        bool CreateImageRecord(string hashedPatientId, string imageFileName, int numImage, out int imageId);
        bool CreateDensityRecord(string hashedPatientId, VolparaDensityMessage densityMessage, out int densityId);
    }
}
