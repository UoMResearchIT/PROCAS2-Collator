using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Services.App
{
    public interface IDashboardService
    {
        int GetTotalParticipantCount();
        int GetConsentedCount();
        int GetConsentedNoDetails();
    }
}
