using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using PROCAS2.Models.ViewModels;
using PROCAS2.Data.Entities;
using PROCAS2.Data;

namespace PROCAS2.Services.App
{
    public class HistologyService:IHistologyService
    {

        private IGenericRepository<Histology> _histologyRepo;
        private IGenericRepository<HistologyFocus> _histologyFocusRepo;
        private IGenericRepository<HistologyLookup> _histologyLookupRepo;
        private IGenericRepository<Participant> _participantRepo;
        private IUnitOfWork _unitOfWork;

        public HistologyService(IUnitOfWork unitOfWork,
                        IGenericRepository<Histology> histologyRepo,
                        IGenericRepository<HistologyFocus> histologyFocusRepo,
                        IGenericRepository<HistologyLookup> histologyLookupRepo,
                        IGenericRepository<Participant> participantRepo)
        {
            _unitOfWork = unitOfWork;
            _histologyFocusRepo = histologyFocusRepo;
            _histologyLookupRepo = histologyLookupRepo;
            _histologyRepo = histologyRepo;
            _participantRepo = participantRepo;
        }


        /// <summary>
        /// Find any histology record associated with this patient and return it in the view model for editing
        /// </summary>
        /// <param name="NHSnumber">NHS number of patient</param>
        /// <returns>Viewmodel (blank fields if no info found)</returns>
        public HistologyEditViewModel FillEditViewModel(string NHSnumber)
        {
            HistologyEditViewModel model = new HistologyEditViewModel();

            model.NHSNumber = NHSnumber;

            Histology hist = _histologyRepo.GetAll().Where(x => x.Participant.NHSNumber == NHSnumber).FirstOrDefault();
            if (hist != null)
            {
                model.Comments = hist.Comments;
                model.DiagnosisDate = hist.DiagnosisDate;
                model.DiagnosisMultiFocal = hist.DiagnosisMultiFocal;
                model.DiagnosisSideId = hist.DiagnosisSideId;
                model.DiagnosisTypeId = hist.DiagnosisTypeId;
                model.HeaderId = hist.Id;
            }

            model.DiagnosisSides = GetLookups("SIDE");
            model.DiagnosisTypes = GetLookups("TYPE");

            return model;
        }

        /// <summary>
        /// Return a list of the lookups of the passed type
        /// </summary>
        /// <param name="lookupType">Lookup type</param>
        /// <returns>List of the lookups</returns>
        public List<HistologyLookup> GetLookups(string lookupType)
        {
            return _histologyLookupRepo.GetAll().Where(x => x.LookupType == lookupType).OrderBy(x=>x.LookupDescription).ToList();
        }

        /// <summary>
        /// Save the histology header
        /// </summary>
        /// <param name="model">The edit view model</param>
        /// <returns>The internal ID of the record if saved successfully, else 0</returns>
        public int SaveHeader(HistologyEditViewModel model)
        {
            try
            {
                bool newRecord = false;
                Histology hist = _histologyRepo.GetAll().Where(x => x.Participant.NHSNumber == model.NHSNumber).FirstOrDefault();
                if (hist == null)
                {
                    hist = new Histology();
                    newRecord = true;
                }

                hist.Comments = model.Comments;
                hist.DiagnosisDate = model.DiagnosisDate;
                hist.DiagnosisMultiFocal = model.DiagnosisMultiFocal;
                hist.DiagnosisSide = _histologyLookupRepo.GetAll().Where(x => x.Id == model.DiagnosisSideId).FirstOrDefault();
                hist.DiagnosisType = _histologyLookupRepo.GetAll().Where(x => x.Id == model.DiagnosisTypeId).FirstOrDefault();
                hist.Participant = _participantRepo.GetAll().Where(x => x.NHSNumber == model.NHSNumber).FirstOrDefault();

                if (newRecord == true)
                {
                    _histologyRepo.Insert(hist);
                }
                else
                {
                    _histologyRepo.Update(hist);
                }

                _unitOfWork.Save();
                return hist.Id;

            }
            catch
            {
                return 0;
            }

        }
    }
}
