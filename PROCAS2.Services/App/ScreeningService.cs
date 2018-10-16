using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using PROCAS2.Models.ServiceBusMessages;
using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Resources;

namespace PROCAS2.Services.App
{
    public class ScreeningService : IScreeningService
    {

        IGenericRepository<Participant> _participantRepo;
        IUnitOfWork _unitOfWork;
        IGenericRepository<ScreeningRecordV1_5_4> _screeningRepo;
        IGenericRepository<Image> _imageRepo;
        IGenericRepository<VolparaDensity> _densityRepo;
        IWebJobParticipantService _participantService;

        public ScreeningService(IGenericRepository<Participant> participantRepo,
                                IUnitOfWork unitOfWork,
                                IGenericRepository<ScreeningRecordV1_5_4> screeningRepo,
                                IGenericRepository<Image> imageRepo,
                                IGenericRepository<VolparaDensity> densityRepo,
                                IWebJobParticipantService participantService)
        {
            _participantRepo = participantRepo;
            _unitOfWork = unitOfWork;
            _screeningRepo = screeningRepo;
            _participantService = participantService;
            _imageRepo = imageRepo;
            _densityRepo = densityRepo;
        }

        /// <summary>
        /// Create the screening record from the service bus message
        /// </summary>
        /// <param name="hashedPatientId">Hashed NHS number</param>
        /// <param name="xlsMessage">The message</param>
        /// <returns>true if created, else false</returns>
        public bool CreateScreeningRecord(string hashedPatientId, ScreeningXlsMessage xlsMessage, int imageId, int densityId)
        {
            try
            {
                ScreeningRecordV1_5_4 record = HydrateScreeningRecordFromViewModel(xlsMessage);

                record.Participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hashedPatientId).FirstOrDefault();
                record.DataDate = DateTime.Now;
               
                if(imageId != 0)
                {
                    record.Image = _imageRepo.GetAll().Where(x => x.Id == imageId).FirstOrDefault();
                }

                if (densityId != 0)
                {
                    record.VolparaDensity = _densityRepo.GetAll().Where(x => x.Id == densityId).FirstOrDefault();
                }

                _screeningRepo.Insert(record);
                _unitOfWork.Save();
                
                // Created the screening record so record it in the audit trail
                _participantService.AddEvent(record.Participant, _participantService.GetSystemUser(EventResources.VOLPARA_AUTO_USER), DateTime.Now, EventResources.EVENT_VOLPARA, EventResources.EVENT_VOLPARA_STR);

            }
            catch (Exception ex)
            {
                _unitOfWork.Reject();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copy the values from the service bus model to the data entity
        /// </summary>
        /// <param name="xlsMessage">service bus model</param>
        /// <returns>data entity</returns>
        private ScreeningRecordV1_5_4 HydrateScreeningRecordFromViewModel(ScreeningXlsMessage xlsMessage)
        {
            ScreeningRecordV1_5_4 record = new ScreeningRecordV1_5_4();


            foreach (PropertyInfo pi in typeof(ScreeningXlsMessage).GetProperties())
            {
                object modelValue = pi.GetValue(xlsMessage); // Get the value from the model
                if (modelValue != null)
                {
                    PropertyInfo recProp = typeof(ScreeningRecordV1_5_4).GetProperty(pi.Name);
                    if (recProp != null)
                    {
                        recProp.SetValue(record, modelValue.ToString());
                    }
                }
            }


            return record;
        }


        /// <summary>
        /// Create the overall density record for the patient.
        /// </summary>
        /// <param name="hashedPatientId">hashed NHS number</param>
        /// <param name="densityMessage">object containing the incoming density message</param>
        /// <returns>true is record created successfully, else false</returns>
        public bool CreateDensityRecord(string hashedPatientId, VolparaDensityMessage densityMessage, out int densityId)
        {
            try
            {
                VolparaDensity record = HydrateDensityRecordFromViewModel(densityMessage);

                record.Participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hashedPatientId).FirstOrDefault();
                record.DataDate = DateTime.Now;

                _densityRepo.Insert(record);
                _unitOfWork.Save();

                densityId = record.Id;

            }
            catch (Exception ex)
            {
                densityId = 0;
                _unitOfWork.Reject();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copy the values from the service bus model to the data entity
        /// </summary>
        /// <param name="densityMessage">service bus model</param>
        /// <returns>data entity</returns>
        private VolparaDensity HydrateDensityRecordFromViewModel(VolparaDensityMessage densityMessage)
        {
            VolparaDensity record = new VolparaDensity();

            // Scorecard results
            record.AverageAppliedForce = densityMessage.ScoreCardResults.AverageAppliedForce;
            record.AverageAppliedPressure = densityMessage.ScoreCardResults.AverageAppliedPressure;
            record.AverageBreastVolume = densityMessage.ScoreCardResults.AverageBreastVolume;
            record.AverageManufacturerDosePerImage = densityMessage.ScoreCardResults.AverageManufacturerDosePerImage;
            record.AverageVolparaDosePerImage = densityMessage.ScoreCardResults.AverageVolparaDosePerImage;
            record.DensityImagesUsedForLccLmloRccRmlo = string.Join(",", densityMessage.ScoreCardResults.DensityImagesUsedForLccLmloRccRmlo);
            record.VolparaDensityGrade4ThEdition = densityMessage.ScoreCardResults.VolparaDensityGrade4ThEdition;
            record.VolparaDensityGrade5ThEdition = densityMessage.ScoreCardResults.VolparaDensityGrade5ThEdition;
            record.VolparaDensityGrade5ThEditionUsingBreastAverage = densityMessage.ScoreCardResults.VolparaDensityGrade5ThEditionUsingBreastAverage;
            record.VolparaDensityPercentageUsingBreastAverage = densityMessage.ScoreCardResults.VolparaDensityPercentageUsingBreastAverage;
            record.VolparaDensityPercentageUsingMaximumBreast = densityMessage.ScoreCardResults.VolparaDensityPercentageUsingMaximumBreast;
            record.RightBreastTotalDose = densityMessage.ScoreCardResults.RightBreastTotalDose;
            record.LeftBreastTotalDose = densityMessage.ScoreCardResults.LeftBreastTotalDose;
            record.DensityOutliers = string.Join(",", densityMessage.ScoreCardResults.DensityOutliers);

            if (densityMessage.ScoreCardResults.LeftBreastFindings != null)
            {
                record.LeftBreastFibroglandularTissueVolume = densityMessage.ScoreCardResults.LeftBreastFindings.FibroglandularTissueVolume;
                record.LeftBreastVolume = densityMessage.ScoreCardResults.LeftBreastFindings.BreastVolume;
                record.LeftBreastVolumetricBreastDensity = densityMessage.ScoreCardResults.LeftBreastFindings.VolumetricBreastDensity;
            }

            if (densityMessage.ScoreCardResults.RightBreastFindings != null)
            {
                record.RightBreastFibroglandularTissueVolume = densityMessage.ScoreCardResults.RightBreastFindings.FibroglandularTissueVolume;
                record.RightBreastVolume = densityMessage.ScoreCardResults.RightBreastFindings.BreastVolume;
                record.RightBreastVolumetricBreastDensity = densityMessage.ScoreCardResults.RightBreastFindings.VolumetricBreastDensity;
            }

            // Volpara server scorecard results
            record.ServerAverageAppliedForce = densityMessage.VolparaServerScoreCardResults.AverageAppliedForce;
            record.ServerAverageAppliedPressure = densityMessage.VolparaServerScoreCardResults.AverageAppliedPressure;
            record.ServerAverageBreastVolume = densityMessage.VolparaServerScoreCardResults.AverageBreastVolume;
            record.ServerAverageManufacturerDosePerImage = densityMessage.VolparaServerScoreCardResults.AverageManufacturerDosePerImage;
            record.ServerAverageVolparaDosePerImage = densityMessage.VolparaServerScoreCardResults.AverageVolparaDosePerImage;
            record.ServerDensityImagesUsedForLccLmloRccRmlo = string.Join(",", densityMessage.VolparaServerScoreCardResults.DensityImagesUsedForLccLmloRccRmlo);
            record.ServerVolparaDensityGrade4ThEdition = densityMessage.VolparaServerScoreCardResults.VolparaDensityGrade4ThEdition;
            record.ServerVolparaDensityGrade5ThEdition = densityMessage.VolparaServerScoreCardResults.VolparaDensityGrade5ThEdition;
            record.ServerVolparaDensityGrade5ThEditionUsingBreastAverage = densityMessage.VolparaServerScoreCardResults.VolparaDensityGrade5ThEditionUsingBreastAverage;
            record.ServerVolparaDensityPercentageUsingBreastAverage = densityMessage.VolparaServerScoreCardResults.VolparaDensityPercentageUsingBreastAverage;
            record.ServerVolparaDensityPercentageUsingMaximumBreast = densityMessage.VolparaServerScoreCardResults.VolparaDensityPercentageUsingMaximumBreast;
            record.ServerRightBreastTotalDose = densityMessage.VolparaServerScoreCardResults.RightBreastTotalDose;
            record.ServerLeftBreastTotalDose = densityMessage.VolparaServerScoreCardResults.LeftBreastTotalDose;
            record.ServerDensityOutliers = string.Join(",", densityMessage.VolparaServerScoreCardResults.DensityOutliers);

            if (densityMessage.VolparaServerScoreCardResults.LeftBreastFindings != null)
            {
                record.ServerLeftBreastFibroglandularTissueVolume = densityMessage.VolparaServerScoreCardResults.LeftBreastFindings.FibroglandularTissueVolume;
                record.ServerLeftBreastVolume = densityMessage.VolparaServerScoreCardResults.LeftBreastFindings.BreastVolume;
                record.ServerLeftBreastVolumetricBreastDensity = densityMessage.VolparaServerScoreCardResults.LeftBreastFindings.VolumetricBreastDensity;
            }

            if (densityMessage.VolparaServerScoreCardResults.RightBreastFindings != null)
            {
                record.ServerRightBreastFibroglandularTissueVolume = densityMessage.VolparaServerScoreCardResults.RightBreastFindings.FibroglandularTissueVolume;
                record.ServerRightBreastVolume = densityMessage.VolparaServerScoreCardResults.RightBreastFindings.BreastVolume;
                record.ServerRightBreastVolumetricBreastDensity = densityMessage.VolparaServerScoreCardResults.RightBreastFindings.VolumetricBreastDensity;
            }

            return record;
        }


        /// <summary>
        /// Create the record that records where the patient's image is.
        /// </summary>
        /// <param name="hashedPatientId">hashed NHS number</param>
        /// <param name="imageFileName">file name of the image in Azure</param>
        /// <returns>true if record created successfully, else false</returns>
        public bool CreateImageRecord(string hashedPatientId, string imageFileName, int numImage, out int imageId)
        {
            try
            {
                Image image = new Image();
                image.Participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hashedPatientId).FirstOrDefault();
                image.OrigName = numImage.ToString();
                image.CurrentName = imageFileName;

                _imageRepo.Insert(image);
                _unitOfWork.Save();

                imageId = image.Id;
            }
            catch(Exception ex)
            {
                imageId = 0;
                _unitOfWork.Reject();
                return false;
            }

            
            return true;
        }
    }
}
