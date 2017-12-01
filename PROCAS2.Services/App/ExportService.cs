using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using PROCAS2.Models.ViewModels;
using PROCAS2.Data;
using PROCAS2.Data.Entities;

namespace PROCAS2.Services.App
{
    public class ExportService:IExportService
    {
        private IUnitOfWork _unitOfWork;
        private IGenericRepository<Participant> _participantRepo;

        public ExportService(IGenericRepository<Participant> participantRepo,
                            IUnitOfWork unitOfWork)
        {
            _participantRepo = participantRepo;
            _unitOfWork = unitOfWork;
        }


        public ExportResultsViewModel GenerateLetters(ExportLettersViewModel model)
        {
            ExportResultsViewModel retModel = new ExportResultsViewModel();

            List<Participant> participants = _participantRepo.GetAll().Where(x => x.SentRisk == false).ToList();
            foreach(Participant participant in participants)
            {
                retModel.Letters.Add(new Letter()
                {
                    LetterText = participant.RiskLetterContent,
                    Name = participant.Title + " " + participant.FirstName + " " + participant.LastName
                });
            }

            return retModel;
        }
    }
}
