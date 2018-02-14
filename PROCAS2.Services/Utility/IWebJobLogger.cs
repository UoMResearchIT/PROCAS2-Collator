using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Services.Utility
{
    public interface IWebJobLogger
    {
        void Log(WebJobLogMessageType messageType, WebJobLogLevel logLevel, string message, string stackTrace = null, string messageBody = null);
        List<WebJobLog> GetAllCurrentErrors();
        int GetLogCount(WebJobLogMessageType messageType, WebJobLogLevel logLevel);
    }
}
