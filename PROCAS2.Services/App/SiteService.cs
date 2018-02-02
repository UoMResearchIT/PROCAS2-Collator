﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;

namespace PROCAS2.Services.App
{
    public class SiteService :ISiteService
    {
        private IGenericRepository<ScreeningSite> _siteRepo;
        private IGenericRepository<Participant> _participantRepo;
        private IUnitOfWork _unitOfWork;

        public SiteService(IGenericRepository<ScreeningSite> siteRepo,
                            IGenericRepository<Participant> participantRepo,
                            IUnitOfWork unitOfWork)
        {
            _siteRepo = siteRepo;
            _participantRepo = participantRepo;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get the number of participants in each site
        /// </summary>
        /// <returns>List of sites</returns>
        public List<SiteNumber> ReturnAllSiteNumbers()
        {
            List<SiteNumber> numbers = new List<SiteNumber>();

            List<ScreeningSite> sites = _siteRepo.GetAll().ToList();

            foreach(ScreeningSite site in sites)
            {
                SiteNumber num = new SiteNumber();
                num.SiteCode = site.Code;
                num.NumberOfParticipants = _participantRepo.GetAll().Where(x => x.ScreeningSite.Code == site.Code).Count();

                numbers.Add(num);
            }

            return numbers;

        }


        /// <summary>
        /// Return an edit view model for the site
        /// </summary>
        /// <param name="code">site code</param>
        /// <returns>site view model</returns>
        public SiteEditViewModel FillEditViewModel(string code)
        {
            ScreeningSite site = _siteRepo.GetAll().Where(x => x.Code == code).FirstOrDefault();
            if (site != null)
            {
                SiteEditViewModel model = new SiteEditViewModel();
                model.Code = site.Code;
                model.AddressLine1 = site.AddressLine1;
                model.AddressLine2 = site.AddressLine2;
                model.AddressLine3 = site.AddressLine3;
                model.AddressLine4 = site.AddressLine4;
                model.LetterFrom = site.LetterFrom;
                
                model.Name = site.Name;
                model.PostCode = site.PostCode;
                model.SigFileName = site.SigFileName;

                return model;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Save the site record with the information from the edit screen
        /// </summary>
        /// <param name="model">view model from the edit screen</param>
        public void SaveSiteRecord(SiteEditViewModel model)
        {
            bool create = false;

            ScreeningSite site = _siteRepo.GetAll().Where(x => x.Code == model.Code).FirstOrDefault();
            if (site == null)
            {
                create = true;
                site = new ScreeningSite();
                site.Code = model.Code;
            }

            site.AddressLine1 = model.AddressLine1;
            site.AddressLine2 = model.AddressLine2;
            site.AddressLine3 = model.AddressLine3;
            site.AddressLine4 = model.AddressLine4;
            site.LetterFrom = model.LetterFrom;
            
            site.Name = model.Name;
            site.PostCode = model.PostCode;
            site.SigFileName = model.SigFileName;

            if (create == true)
            {
                _siteRepo.Insert(site);
            }
            else
            {
                _siteRepo.Update(site);
            }

            _unitOfWork.Save();
        }

        /// <summary>
        /// Delete the site with the passed code
        /// </summary>
        public bool DeleteSite(string code)
        {
            ScreeningSite site = _siteRepo.GetAll().Where(x => x.Code == code).FirstOrDefault();
            if (site != null)
            {
                _siteRepo.Delete(site);
                _unitOfWork.Save();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
