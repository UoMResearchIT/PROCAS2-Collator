using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Services.Utility
{
    public interface IConfigService
    {
        string GetAppSetting(string key);
        
        int? GetIntAppSetting(string key);
        
        DateTime? GetDateTimeAppSetting(string key);
    }
}
