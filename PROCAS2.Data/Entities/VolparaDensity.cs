using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class VolparaDensity
    {
        public int Id { get; set; }

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; }

        // Start of ScorecardResults
        [MaxLength(12)]
        public string LeftBreastFibroglandularTissueVolume { get; set; }

        [MaxLength(12)]
        public string LeftBreastVolume { get; set; }

        [MaxLength(12)]
        public string LeftBreastVolumetricBreastDensity { get; set; }

        [MaxLength(12)]
        public string RightBreastFibroglandularTissueVolume { get; set; }

        [MaxLength(12)]
        public string RightBreastVolume { get; set; }

        [MaxLength(12)]
        public string RightBreastVolumetricBreastDensity { get; set; }

        [MaxLength(12)]
        public string VolparaDensityPercentageUsingMaximumBreast { get; set; }

        [MaxLength(12)]
        public string VolparaDensityPercentageUsingBreastAverage { get; set; }

        [MaxLength(12)]
        public string VolparaDensityGrade4ThEdition { get; set; }

        [MaxLength(12)]
        public string VolparaDensityGrade5ThEdition { get; set; }

        [MaxLength(12)]
        public string VolparaDensityGrade5ThEditionUsingBreastAverage { get; set; }

        [MaxLength(12)]
        public string AverageBreastVolume { get; set; }

        [MaxLength(12)]
        public string AverageAppliedPressure { get; set; }

        [MaxLength(12)]
        public string AverageAppliedForce { get; set; }

        [MaxLength(12)]
        public string AverageManufacturerDosePerImage { get; set; }

        [MaxLength(12)]
        public string AverageVolparaDosePerImage { get; set; }

        [MaxLength(12)]
        public string LeftBreastTotalDose { get; set; }

        [MaxLength(12)]
        public string RightBreastTotalDose { get; set; }

        [MaxLength(20)]
        public string DensityOutliers { get; set; }

        [MaxLength(200)]
        public string DensityImagesUsedForLccLmloRccRmlo { get; set; }
        // End of Scorecard results

        // Start of Volpara server scorecard results
        [MaxLength(12)]
        public string ServerLeftBreastFibroglandularTissueVolume { get; set; }

        [MaxLength(12)]
        public string ServerLeftBreastVolume { get; set; }

        [MaxLength(12)]
        public string ServerLeftBreastVolumetricBreastDensity { get; set; }

        [MaxLength(12)]
        public string ServerRightBreastFibroglandularTissueVolume { get; set; }

        [MaxLength(12)]
        public string ServerRightBreastVolume { get; set; }

        [MaxLength(12)]
        public string ServerRightBreastVolumetricBreastDensity { get; set; }

        [MaxLength(12)]
        public string ServerVolparaDensityPercentageUsingMaximumBreast { get; set; }

        [MaxLength(12)]
        public string ServerVolparaDensityPercentageUsingBreastAverage { get; set; }

        [MaxLength(12)]
        public string ServerVolparaDensityGrade4ThEdition { get; set; }

        [MaxLength(12)]
        public string ServerVolparaDensityGrade5ThEdition { get; set; }

        [MaxLength(12)]
        public string ServerVolparaDensityGrade5ThEditionUsingBreastAverage { get; set; }

        [MaxLength(12)]
        public string ServerAverageBreastVolume { get; set; }

        [MaxLength(12)]
        public string ServerAverageAppliedPressure { get; set; }

        [MaxLength(12)]
        public string ServerAverageAppliedForce { get; set; }

        [MaxLength(12)]
        public string ServerAverageManufacturerDosePerImage { get; set; }

        [MaxLength(12)]
        public string ServerAverageVolparaDosePerImage { get; set; }

        [MaxLength(12)]
        public string ServerLeftBreastTotalDose { get; set; }

        [MaxLength(12)]
        public string ServerRightBreastTotalDose { get; set; }

        [MaxLength(20)]
        public string ServerDensityOutliers { get; set; }

        [MaxLength(200)]
        public string ServerDensityImagesUsedForLccLmloRccRmlo { get; set; }
        // End of Volpara server scorecard results

        public bool UsingScoreCard { get; set; }
        public DateTime DataDate { get; set; }
    }
}
