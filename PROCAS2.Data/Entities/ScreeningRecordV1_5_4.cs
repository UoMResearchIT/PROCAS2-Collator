using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class ScreeningRecordV1_5_4
    {
        public int Id { get; set; }

        public DateTime DataDate { get; set; }

        [MaxLength(3)]
        public string MammoImageType { get; set; }

        [MaxLength(100)]
        public string RequestedProcedure { get; set; }


        public DateTime? Timestamp { get; set; }

        [MaxLength(50)]
        public string OSName { get; set; }

        [MaxLength(20)]
        public string OSVersion { get; set; }

        [MaxLength(8)]
        public string CurrentCulture { get; set; }

        [MaxLength(8)]
        public string InstalledUICulture { get; set; }

        [MaxLength(3)] 
        public string WriteOutDisplayImage { get; set; }

        [MaxLength(30)]
        public string VolparaVersion { get; set; }

        [MaxLength(100)]
        public string DICOMTAGManufacturer { get; set; }

        [MaxLength(100)]
        public string DICOMTagDeviceSerialNumber { get; set; }

        [MaxLength(100)]
        public string DICOMTagDetectorID { get; set; }

        [MaxLength(20)]
        public string MaxAllowedKVP { get; set; }

        public int? BTSF { get; set; }

        public int? BTSTF { get; set; }

        [MaxLength(3)]
        public string PartialView { get; set; }

        
        public double? CompressionPlateSlant { get; set; }

        [MaxLength(30)] 
        public string DetectorType { get; set; }

        [MaxLength(3)] 
        public string DoFlatFieldCorrection { get; set; }

        
        public double? DoFatFieldCorrection { get; set; }

       
        public double? Gain { get; set; }

        
        public double? NativePixelSize { get; set; }

        
        public double? Offset { get; set; }

        
        public double? SourceToDetector { get; set; }

        
        public double? SupportToDetector { get; set; }

        [MaxLength(20)] 
        public string TubeType { get; set; }

        [MaxLength(3)] 
        public string UseFatWedge { get; set; }

        [MaxLength(5)] 
        public string UseNewSlantAlgorithm { get; set; }

        [MaxLength(3)] 
        public string ValidToString { get; set; }

      
        public double? WAgDefaultFilterThickness { get; set; }

      
        public double? WRhDefaultFilterThickness { get; set; }

        [MaxLength(5)]
        public string BreastSide { get; set; }

        [MaxLength(5)]
        public string ChestPosition { get; set; }

        [MaxLength(10)]
        public string PectoralPosition { get; set; }

        [MaxLength(3)]
        public string MammoView { get; set; }

        public DateTime? StudyDate { get; set; }

        [MaxLength(50)]
        public string OperatorName { get; set; }

        public DateTime? PatientDOB { get; set; }

        public int? PatientAge { get; set; }

        [MaxLength(20)]
        public string PatientID { get; set; }

        [MaxLength(20)]
        public string DetectorID { get; set; }

        [MaxLength(20)]
        public string XRaySystem { get; set; }

        public double? TargetPixelSize { get; set; }

        [MaxLength(20)]
        public string NearestNeighbourResample { get; set; }

        public double? ResizedPixelSizeMm { get; set; }

        [MaxLength(8)]
        public string PectoralSide { get; set; }

        [MaxLength(8)]
        public string MedialSide { get; set; }

        [MaxLength(20)]
        public string PaddleType { get; set; }

        public int? ExposureMas { get; set; }

        public int? ExposureTimeMs { get; set; }

        [MaxLength(10)]
        public string TargetMaterial { get; set; }

        [MaxLength(10)]
        public string FilterMaterial { get; set; }

        public double? FilterThicknessMm { get; set; }

        public int? TubeVoltageKvp { get; set; }

        [MaxLength(1000)]
        public string Special { get; set; }

        public double? HVLMm { get; set; }

        public int? CompressionForceN { get; set; }

        public int? RecordedBreastThicknessMm { get; set; }

        [MaxLength(200)]
        public string InnerBreastStatistics { get; set; }

        public double? muFatPerMm { get; set; }

        public double? MethodAllPlaneFit { get; set; }

        [MaxLength(200)]
        public string RejectingMethod1Because { get; set; }

        public double? MethodFatPlaneFit { get; set; }

        public double? CalculatedSigma { get; set; }

        public double? ComputedSlantAngle { get; set; }

        public double? ComputedSlantMm { get; set; }

        public int? ComputedBreastThickness { get; set; }

        public double? ScatterScaleFactor { get; set; }

        [MaxLength(20)]
        public string Scatter { get; set; }

        public double? SegPhaseDE { get; set; }
        public double? SegPhaseOD { get; set; }
        public double? SegPhaseBE { get; set; }
        public double? SegPhasePA { get; set; }
        public double? SegPhaseOA { get; set; }
        public double? SegPhaseUA { get; set; }
        public double? SegPhasePD { get; set; }
        public double? SegSphereDE { get; set; }
        public double? SegSphereOD { get; set; }
        public double? SegSphereBE { get; set; }
        public double? SegSpherePA { get; set; }
        public double? SegSphereOA { get; set; }
        public double? SegSphereUA { get; set; }
        public double? SegSpherePD { get; set; }

        public double? ContactAreaMm2 { get; set; }

        public double? CompressionPressureKPa { get; set; }

        [MaxLength(200)]
        public string SettingDTto89 { get; set; }

        [MaxLength(20)]
        public string PFAT_Edge_Zone { get; set; }

        [MaxLength(20)]
        public string HintRejectlevel { get; set; }

        [MaxLength(20)]
        public string HintIgnoreLevel { get; set; }

        public double? EntranceDoseInMgy { get; set; }

        public double? EstimatedEntranceDoseInMgy { get; set; }

        [MaxLength(1000)]
        public string Warning { get; set; }

        public double? GlandularityPercent { get; set; }

        public double? VolparaMeanGlandularDoseInMgy { get; set; }

        public double? FiftyPercentGlandularDoseInMgy { get; set; }

        public double? OrganDose { get; set; }

        public double? OrganDoseInMgy { get; set; }

        [MaxLength(200)]
        public string Method2Results { get; set; }

        [MaxLength(200)]
        public string CorrectionComplete { get; set; }

        public double? NippleConfidence { get; set; }

        [MaxLength(200)]
        public string NippleConfidenceMessage { get; set; }

        [MaxLength(3)]
        public string NippleInProfile { get; set; }

        public double? NippleDistanceFromMedialEdgeInMm { get; set; }

        public double? NippleDistanceFromPosteriorEdgeInMm { get; set; }

        public double? NippleCenterDistanceFromMedialEdgeInMm { get; set; }

        public double? NippleCenterDistanceFromPosteriorEdgeInMm { get; set; }

        public double? CCPosteriorNippleLineLengthInMm { get; set; }

        public double? NippleMedialLateralDistanceInMm { get; set; }

        public double? NippleMedialLateralAngleInDegrees { get; set; }

        public double? BreastEdgeDistanceToPosteriorMedialCornerInMm { get; set; }

        public double? BreastEdgeDistanceToPosteriorLateralCornerInMm { get; set; }

        [MaxLength(3)]
        public string CleavageDetected { get; set; }

        [MaxLength(3)]
        public string ShoulderDetected { get; set; }

        public double? MeanDenseThicknessInMm { get; set; }

        public double? MaximumDenseThicknessInMm { get; set; }

        public double? SDDenseThicknessInMm { get; set; }

        public double? MaximumDenseThicknessDistanceFromMedialEdgeInMm { get; set;}

        public double? MaximumDenseThicknessDistanceFromPosteriorEdgeInMm { get; set; }

        public double? DensityMapAttenuatingPixelCount { get; set; }

        public double? MaximumPercentDensityIn1Cm2Area { get; set; }

        public double? MaximumDenseVolumeIn1Cm2AreaInCm3 { get; set; }

        public double? MaximumDensity1Cm2AreaDistanceFromMedialEdgeInMm { get; set; }

        public double? MaximumDensity1Cm2AreaDistanceFromPosteriorEdgeInMm { get; set; }

        public double? DenseAreaPercent { get; set; }

        public double? AreaGreaterThan10mmDenseMm2 { get; set; }

        public double? HintVolumeCm3 { get; set; }

        public double? BreastVolumeCm3 { get; set; }

        public double? VolumetricBreastDensity { get; set; }

        public double? Out_BreastVolume { get; set; }

        public double? Out_FGTV { get; set; }

        public double? Out_Density { get; set; }

        [MaxLength(200)]
        public string Run_Information { get; set; }

        [MaxLength(200)]
        public string VolparaOkay { get; set; }

        //-----


        //[MaxLength(10)]
        //public string Detector { get; set; }

        //[MaxLength(100)]
        //public string ManufacturerModelName { get; set; }

        //public DateTime? AccessionDate { get; set; }

        //public DateTime? ContentDate { get; set; }

        //[MaxLength(15)]
        //public string AccessionNumber { get; set; }

        //[MaxLength(40)]
        //public string StudyInstanceUID { get; set; }

        //[MaxLength(40)]
        //public string SeriesInstanceUID { get; set; }

        //public int? InstanceNumber { get; set; }

        //public int? XRayTubeCurrent { get; set; }

        //public double? DetectorTemperature { get; set; }

        //public int? PectoralAngleDegrees { get; set; }

        //public int? PositionerPrimaryAngle { get; set; }

        //public double? DetectorPrimaryAngle { get; set; }

        //public double? DetectorSecondaryAngle { get; set; }

        //[MaxLength(100)]
        //public string ReferringPhysicianName { get; set; }

        //[MaxLength(1000)]
        //public string VolparaError { get; set; }

        //[MaxLength(100)]
        //public string ImageQualityDiagnostics { get; set; }

        //public double? NippleAreaInMm2 { get; set; }

        //public double? PercentNippleAreaAnteriorToSkinBoundary { get; set; }

        //[MaxLength(10)]
        //public string NippleCenterLocationRelativeToSkinLine { get; set; }

        //public double? NippleToInferiorPectoralMuscleVerticalLengthInMm { get; set; }

        //public double? PNLToInferiorPectoralMuscleVerticalLengthInMm { get; set; }

        //public double? MLOPosterierNippleLineLengthInMm { get; set; }

        //public double? NippleLineLengthInMm { get; set; }

        //[MaxLength(3)]
        //public string InframammaryFoldVisible { get; set; }

        //public double? InframammaryFoldAreaInMm2 { get; set; }

        //public double? InframammaryFoldDistanceFromPosteriorEdgeInMm { get; set; }

        //public double? InframammaryFoldDistanceFromSuperiorEdgeInMm { get; set; }

        //public double? SuperiorPectoralWidthInMm { get; set; }

        //public double? PosteriorPectoralLengthInMm { get; set; }

        //[MaxLength(10)]
        //public string PectoralShape { get; set; }

        //public double? NippleDistanceFromSuperiorEdgeInMm { get; set; }

        //public double? BreastCenterToImageCenterDistanceInMm { get; set; }

        //public double? DoseCalibrationError { get; set; }

        //public double? MeasuredHVLInMmAl { get; set; }

        //public double? MeasuredEntranceDoseInMgy { get; set; }

        //public DateTime? DoseCalibrationUsedDateTime { get; set; }

        //public double? DoseCalculationBasis { get; set; }

        public virtual Image Image { get; set; }

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; }
    }
}
