using System;
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
                model.TrustCode = site.TrustCode;
                model.Telephone = site.Telephone;
                
                model.Name = site.Name;
                model.PostCode = site.PostCode;
                model.Signature = site.Signature;

                model.LogoFooterLeft = site.LogoFooterLeft;
                model.LogoFooterLeftHeight = String.IsNullOrEmpty(site.LogoFooterLeftHeight)?0:Convert.ToDouble(site.LogoFooterLeftHeight);
                model.LogoFooterLeftWidth = String.IsNullOrEmpty(site.LogoFooterLeftWidth) ? 0 : Convert.ToDouble(site.LogoFooterLeftWidth);

                model.LogoFooterRight = site.LogoFooterRight;
                model.LogoFooterRightHeight = String.IsNullOrEmpty(site.LogoFooterRightHeight) ? 0 : Convert.ToDouble(site.LogoFooterRightHeight);
                model.LogoFooterRightWidth = String.IsNullOrEmpty(site.LogoFooterRightWidth) ? 0 : Convert.ToDouble(site.LogoFooterRightWidth);

                model.LogoHeaderRight = site.LogoHeaderRight;
                model.LogoHeaderRightHeight = String.IsNullOrEmpty(site.LogoHeaderRightHeight) ? 0 : Convert.ToDouble(site.LogoHeaderRightHeight);
                model.LogoHeaderRightWidth = String.IsNullOrEmpty(site.LogoHeaderRightWidth) ? 0 : Convert.ToDouble(site.LogoHeaderRightWidth);

                model.FamilyHistoryClinic = site.FamilyHealthClinic;

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
            site.TrustCode = model.TrustCode;
            site.Telephone = model.Telephone;
            
            site.Name = model.Name;
            site.PostCode = model.PostCode;
            site.Signature = model.Signature;
            site.LogoFooterLeft = model.LogoFooterLeft;
            site.LogoFooterRight = model.LogoFooterRight;
            site.LogoHeaderRight = model.LogoHeaderRight;
            site.LogoFooterLeftHeight = model.LogoFooterLeftHeight.ToString();
            site.LogoFooterLeftWidth = model.LogoFooterLeftWidth.ToString();
            site.LogoFooterRightHeight = model.LogoFooterRightHeight.ToString();
            site.LogoFooterRightWidth = model.LogoFooterRightWidth.ToString();
            site.LogoHeaderRightHeight = model.LogoHeaderRightHeight.ToString();
            site.LogoHeaderRightWidth = model.LogoHeaderRightWidth.ToString();
            site.FamilyHealthClinic = model.FamilyHistoryClinic;

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
