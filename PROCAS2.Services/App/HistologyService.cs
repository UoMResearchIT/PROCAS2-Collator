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
    public class HistologyService : IHistologyService
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

                model.HistologyFoci = hist.HistologyFoci.ToList();
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
            return _histologyLookupRepo.GetAll().Where(x => x.LookupType == lookupType).OrderBy(x => x.LookupDescription).ToList();
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

        /// <summary>
        /// Fill the focus edit view model
        /// </summary>
        /// <param name="NHSnumber">NHSnumber of participant</param>
        /// <param name="headerId">Histology header ID</param>
        /// <param name="primary">1 = First Primary, 2 = Second Primary</param>
        /// <param name="focusId">ID of focus record (0 if new)</param>
        /// <returns>The view model</returns>
        public HistologyFocusViewModel FillEditFocusViewModel(string NHSnumber, int headerId, int primary, int focusId)
        {
            HistologyFocusViewModel model = new HistologyFocusViewModel();

            model.NHSNumber = NHSnumber;
            model.Id = focusId;
            model.HistologyId = headerId;
            model.PrimaryNumber = primary;

            HistologyFocus focus = _histologyFocusRepo.GetAll().Where(x => x.Id == focusId).FirstOrDefault();
            if (focus != null)
            {
                model.DCISGradeId = focus.DCISGradeId;
                model.ERScore = focus.ERScore;
                model.ERStatus = focus.ERStatus;
                model.FocusNumber = focus.FocusNumber;
                model.HER2Score = focus.HER2Score;
                model.HER2Status = focus.HER2Status;
                model.InvasiveGrade = focus.InvasiveGrade;
                model.InvasiveId = focus.InvasiveId;
                model.InvasiveTumourSize = focus.InvasiveTumourSize;
                model.KISixtySeven = focus.KISixtySeven;
                model.LN2 = focus.LN2;
                model.LymphNodes = focus.LymphNodes;
                model.PathologyId = focus.PathologyId;
                model.PRScore = focus.PRScore;
                model.PRStatus = focus.PRStatus;
                model.VascularInvasionId = focus.VascularInvasionId;
                model.WholeTumourSize = focus.WholeTumourSize;
            }


            model.DCISGrades = GetLookups("DCIS");
            model.Invasives = GetLookups("INVASIVE");
            model.Pathologies = GetLookups("PATH");
            model.VascularInvasions = GetLookups("VASCULAR");


            return model;
        }

        /// <summary>
        /// Save the histology focus
        /// </summary>
        /// <param name="model">Focus edit view model</param>
        /// <returns>ID of created/updated focus record</returns>
        public int SaveFocus(HistologyFocusViewModel model)
        {
            try
            {
                bool newRecord = false;

                HistologyFocus focus = _histologyFocusRepo.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
                if (focus == null)
                {
                    newRecord = true;
                    focus = new HistologyFocus();
                }


                focus.Histology = _histologyRepo.GetAll().Where(x => x.Id == model.HistologyId).FirstOrDefault();
                focus.DCISGrade = _histologyLookupRepo.GetAll().Where(x => x.Id == model.DCISGradeId).FirstOrDefault();
                focus.ERScore = model.ERScore;
                focus.ERStatus = model.ERStatus;
                focus.FocusNumber = model.FocusNumber;
                focus.HER2Score = model.HER2Score;
                focus.HER2Status = model.HER2Status;
                focus.Invasive = _histologyLookupRepo.GetAll().Where(x => x.Id == model.InvasiveId).FirstOrDefault();
                focus.InvasiveGrade = model.InvasiveGrade;
                focus.InvasiveTumourSize = model.InvasiveTumourSize;
                focus.KISixtySeven = model.KISixtySeven;
                focus.LN2 = model.LN2;
                focus.LymphNodes = model.LymphNodes;
                focus.Pathology = _histologyLookupRepo.GetAll().Where(x => x.Id == model.PathologyId).FirstOrDefault();
                focus.PrimaryNumber = model.PrimaryNumber;
                focus.PRScore = model.PRScore;
                focus.PRStatus = model.PRStatus;
                focus.VascularInvasion = _histologyLookupRepo.GetAll().Where(x => x.Id == model.VascularInvasionId).FirstOrDefault();
                focus.WholeTumourSize = model.WholeTumourSize;


                if (newRecord == true)
                {
                    _histologyFocusRepo.Insert(focus);
                }
                else
                {
                    _histologyFocusRepo.Update(focus);
                }

                _unitOfWork.Save();
                return focus.Id;


            }
            catch
            {
                return 0;
            }
        }
    }
}
