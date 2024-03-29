﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using PROCAS2.Data;
using PROCAS2.Data.Entities;

namespace PROCAS2.Services.Utility
{
    public class ConfigService:IConfigService
    {

        private IGenericRepository<AppNewsItem> _appNewsRepo;

        public ConfigService(IGenericRepository<AppNewsItem> appNewsRepo )
        {
            _appNewsRepo = appNewsRepo;
        }

        /// <summary>
        /// Return app setting from the web.config. 
        /// </summary>
        /// <param name="key">name of the app setting</param>
        /// <returns>value, or null if not found</returns>
        public string GetAppSetting(string key)
        {
            string retVal = ConfigurationManager.AppSettings[key];

            return retVal;
        }

        /// <summary>
        /// Return app setting from the web.config and converts to int
        /// </summary>
        /// <param name="key">name of app setting</param>
        /// <returns>Value, or null if not found</returns>
        public int? GetIntAppSetting(string key)
        {
            string retVal = ConfigurationManager.AppSettings[key];

            if (String.IsNullOrEmpty(retVal) == true)
            {
                return null;
            }
            else
            {
                int retInt;
                if (Int32.TryParse(retVal, out retInt) == false)
                    return null;
                else
                    return retInt;
            }
            
        }

        /// <summary>
        /// Return app setting from the web.config and converts to DateTime
        /// </summary>
        /// <param name="key">name of app setting</param>
        /// <returns>Value, or null if not found</returns>
        public DateTime? GetDateTimeAppSetting(string key)
        {
            string retVal = ConfigurationManager.AppSettings[key];

            if (String.IsNullOrEmpty(retVal) == true)
            {
                return null;
            }
            else
            {
                DateTime retDate;
                if (DateTime.TryParse(retVal, out retDate) == false)
                    return null;
                else
                    return retDate;
            }

        }

        /// <summary>
        /// Return connection string from the web.config. 
        /// </summary>
        /// <param name="key">name of the connection string</param>
        /// <returns>value, or null if not found</returns>
        public string GetConnectionString(string key)
        {
            string retVal = ConfigurationManager.ConnectionStrings[key].ConnectionString;

            return retVal;
        }

        /// <summary>
        /// Check for the Phase a1 message.
        /// </summary>
        /// <returns>If the phase 1a message is there return true, else false</returns>
        public bool IsPhase1a()
        {
            AppNewsItem item = _appNewsRepo.GetAll().Where(x => x.Message.Contains("*PILOT PHASE 1*")).FirstOrDefault();
            if (item != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
