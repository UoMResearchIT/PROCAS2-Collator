using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PROCAS2.Services.Utility
{
    public interface IStorageService
    {
        bool ProcessConsentPDF(string PDF, string filename);
        bool StoreCRAMessage(string message, string filename);
        bool StoreVolparaMessage(string message, string filename);
        bool StoreInviteMessage(string studyNumber, string hashedNHSNumber);
        MemoryStream GetConsentForm(int studyNumber);
    }
}
