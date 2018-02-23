using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;

namespace PROCAS2.Services.Utility
{

    public enum WebJobLogLevel
    {
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public enum WebJobLogMessageType
    {
        CRA_Consent = 1,
        CRA_Survey = 2
    }

    public class WebJobLogger : IWebJobLogger
    {

        private IUnitOfWork _unitOfWork;
        private IGenericRepository<WebJobLog> _logRepo;
        private IConfigService _configService;

        public WebJobLogger(IUnitOfWork unitOfWork,
                            IGenericRepository<WebJobLog> logRepo,
                            IConfigService configService)
        {
            _unitOfWork = unitOfWork;
            _logRepo = logRepo;
            _configService = configService;
        }

        /// <summary>
        /// Method for recording log messages to the webjob log table
        /// </summary>
        /// <param name="messageType">What type of service bus message</param>
        /// <param name="logLevel">Level: Info, Warning or Error</param>
        /// <param name="message">The error/info message</param>
        /// <param name="stackTrace">optional stacktrace</param>
        /// <param name="messageBody">optional servicebus message body</param>
        public string  Log(WebJobLogMessageType messageType, WebJobLogLevel logLevel, string message, string stackTrace = null, string messageBody = null)
        {

            int minimumLogLevel;

            // Just store messages with a verbosity rating above the minimum configured.
            if (Int32.TryParse(_configService.GetAppSetting("CRAMinLogLevel"), out minimumLogLevel) == true &&
                    Convert.ToInt32(logLevel) >= minimumLogLevel)
            {
                WebJobLog log = new WebJobLog();
                log.LogLevel = Convert.ToInt32(logLevel);
                log.LogDate = DateTime.Now;
                log.Message = message;
                log.MessageType = Convert.ToInt32(messageType);
                log.StackTrace = stackTrace;
                log.MessageBody = messageBody;
                log.Reviewed = false;

                _logRepo.Insert(log);
                _unitOfWork.Save();
                return message;
            }
            else
                return null;
        }

        /// <summary>
        /// Get the number of unreviewed log records at a certain log level
        /// </summary>
        /// <param name="messageType">the service bus message type</param>
        /// <param name="logLevel">The minimum log level</param>
        /// <returns>number of records above that log level</returns>
        public int GetLogCount(WebJobLogMessageType messageType, WebJobLogLevel logLevel)
        {
            int mType = Convert.ToInt32(messageType);
            int lLevel = Convert.ToInt32(logLevel);
            return _logRepo.GetAll().Where(x => x.Reviewed == false && x.MessageType == mType  && x.LogLevel >= lLevel).Count();
        }

        /// <summary>
        /// Return a list of all current unreviewed errors
        /// </summary>
        /// <returns>the list of errors in date order</returns>
        public List<WebJobLog> GetAllCurrentErrors()
        {
            return _logRepo.GetAll().Where(x => x.Reviewed == false).OrderBy(x => x.LogDate).ToList();
        }

        /// <summary>
        /// Marked the log record with the passed Id as reviewed
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>true if successful, else false</returns>
        public bool Review(int id)
        {
            try
            {
                WebJobLog log = _logRepo.GetAll().Where(x => x.Id == id).FirstOrDefault();
                if (log != null)
                {
                    log.Reviewed = true;
                    _logRepo.Update(log);
                    _unitOfWork.Save();
                }
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Hydrate the TxErrors details for display
        /// </summary>
        /// <param name="id">ID of log record</param>
        /// <returns>View Model</returns>
        public TxErrorsDetailsViewModel FillDetailsViewModel(int id)
        {
            TxErrorsDetailsViewModel model = new TxErrorsDetailsViewModel();

            WebJobLog log = _logRepo.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (log != null)
            {
                model.LogDate = log.LogDate;
                model.LogLevel = log.LogLevel;
                model.Message = log.Message;
                model.MessageBody = log.MessageBody;
                model.MessageType = log.MessageType;
                model.Reviewed = log.Reviewed;
                model.StackTrace = log.StackTrace;
            }

            return model;
        }
    }
}
