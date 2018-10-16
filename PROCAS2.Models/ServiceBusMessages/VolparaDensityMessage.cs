using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Models.ServiceBusMessages
{

    public class LeftBreastFindings
    {
        public string FibroglandularTissueVolume { get; set; }
        public string BreastVolume { get; set; }
        public string VolumetricBreastDensity { get; set; }
    }

    public class RightBreastFindings
    {
        public string FibroglandularTissueVolume { get; set; }
        public string BreastVolume { get; set; }
        public string VolumetricBreastDensity { get; set; }
    }

    public class ScoreCardMessage
    {

        public LeftBreastFindings LeftBreastFindings { get; set; }
        public RightBreastFindings RightBreastFindings { get; set; }


        public string VolparaDensityPercentageUsingMaximumBreast { get; set; }     
        public string VolparaDensityPercentageUsingBreastAverage { get; set; }    
        public string VolparaDensityGrade4ThEdition { get; set; }    
        public string VolparaDensityGrade5ThEdition { get; set; }      
        public string VolparaDensityGrade5ThEditionUsingBreastAverage { get; set; }       
        public string AverageBreastVolume { get; set; }     
        public string AverageAppliedPressure { get; set; }     
        public string AverageAppliedForce { get; set; }     
        public string AverageManufacturerDosePerImage { get; set; }     
        public string AverageVolparaDosePerImage { get; set; }
        public string LeftBreastTotalDose { get; set; }
        public string RightBreastTotalDose { get; set; }

        public List<string> DensityOutliers { get; set; }
        public List<string> DensityImagesUsedForLccLmloRccRmlo { get; set; }
    }

    public class VolparaDensityMessage
    {
        public ScoreCardMessage ScoreCardResults { get; set; }
        public ScoreCardMessage VolparaServerScoreCardResults { get; set; }
    }
}
