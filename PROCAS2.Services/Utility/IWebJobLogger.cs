using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;

namespace PROCAS2.Services.Utility
{
    public interface IWebJobLogger
    {
        string Log(WebJobLogMessageType messageType, WebJobLogLevel logLevel, string message, string stackTrace = null, string messageBody = null);
        List<WebJobLog> GetAllCurrentErrors();
        int GetLogCount(WebJobLogMessageType messageType, WebJobLogLevel logLevel);
        bool Review(int id);
        TxErrorsDetailsViewModel FillDetailsViewModel(int id);
    }
}
