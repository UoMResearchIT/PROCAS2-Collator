﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using PROCAS2.Data;
using PROCAS2.Data.Entities;

namespace PROCAS2.Services.Utility
{
    /// <summary>
    /// Methods for User query/creation in the PROCAS2 AppUser database. This extra table is mainly for user profile stuff.
    /// </summary>
    public class PROCASUserManager:IPROCAS2UserManager
    {

        private IContextService _contextService;
        private IUnitOfWork _unitOfWork;
        private IGenericRepository<AppUser> _appUserRepo;

        public PROCASUserManager(IContextService contextService, IUnitOfWork unitOfWork,
                                IGenericRepository<AppUser> appUserRepo)
        {
            _contextService = contextService;
            _unitOfWork = unitOfWork;
            _appUserRepo = appUserRepo;
        }

        /// <summary>
        /// Check to see if the current user exists in the AppUser table!
        /// </summary>
        /// <returns>false if record does not exist or user is not active, else true</returns>
        public bool CheckUserRecord(string userName)
        {
            
            if (!String.IsNullOrEmpty(userName)) 
            {
                
                AppUser appUser = _appUserRepo.GetAll().Where(x => x.UserCode == userName).FirstOrDefault();
                if (appUser == null) // appUser does not exist
                {
                    return false;
                }
                else
                {
                    if (appUser.Active == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            
            return false;
        }

        /// <summary>
        /// Return all the app users on the system
        /// </summary>
        /// <returns>The list of app users</returns>
        public List<AppUser> GetAllAppUsers()
        {
            return _appUserRepo.GetAll().ToList();
        }
    }
}
