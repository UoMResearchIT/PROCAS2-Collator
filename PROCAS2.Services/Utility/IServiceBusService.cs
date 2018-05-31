using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Services.Utility
{
    public interface IServiceBusService
    {
        bool PostServiceBusMessage(string keyNameAppSetting, string keyValueAppSetting, string baseURLAppSetting, string message, string queueAppSetting, bool compress);
        string GetServiceBusMessage(string keyNameAppSetting, string keyValueAppSetting, string baseURLAppSetting, string queueAppSetting);
    }
}
