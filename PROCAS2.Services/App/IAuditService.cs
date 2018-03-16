using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Services.App
{
    public interface IAuditService
    {
        bool ChangeEventBool(Participant participant, string propertyName, bool oldValue, bool newValue, string reason);
        string ChangeEventString(Participant participant, string propertyName, string oldValue, string newValue, string reason);
        DateTime ChangeEventDate(Participant participant, string propertyName, DateTime oldValue, DateTime newValue, string reason);
        int? ChangeEventInt(Participant participant, string propertyName, int? oldValue, int? newValue, string reason);

        bool AddEvent(Participant participant, AppUser user, DateTime eventDate, string eventCode, string eventNotes, string reason = null);
    }
}
