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

        // Fields common to both CC and MLO MammoViews
        [MaxLength(3)]
        public string MammoImageType { get; set; }
        [MaxLength(50)]
        public string RequestedProcedure { get; set; }
        public string Folder { get; set; }
        [MaxLength(20)]
        public string Timestamp { get; set; }
        [MaxLength(100)]
        public string OSName { get; set; }
        [MaxLength(100)]
        public string OSVersion { get; set; }
        [MaxLength(10)]
        public string CurrentCulture { get; set; }
        [MaxLength(10)]
        public string InstalledUICulture { get; set; }
        [MaxLength(3)]
        public string WriteOutDisplayImage { get; set; }
        [MaxLength(50)]
        public string VolparaVersion { get; set; }
        [MaxLength(50)]
        public string DICOMTAGManufacturer { get; set; }
        [MaxLength(50)]
        public string DICOMTAGDeviceSerialNumber { get; set; }
        [MaxLength(50)]
        public string DICOMTagDetector_ID { get; set; }
        [MaxLength(50)] // Number
        public string MaxAllowedKVP { get; set; }
        [MaxLength(12)] // Number
        public string BTSF { get; set; }
        [MaxLength(12)] // Number
        public string BTSTF { get; set; }
        [MaxLength(50)]
        public string DetectorType { get; set; }
        [MaxLength(3)]
        public string DoFlatFieldCorrection { get; set; }
        [MaxLength(12)] // Number
        public string FSensitivity { get; set; }
        [MaxLength(12)] // Number
        public string Gain { get; set; }
        [MaxLength(12)] // Number
        public string NativePixelSize { get; set; }
        [MaxLength(12)] // Number
        public string Offset { get; set; }
        [MaxLength(3)]
        public string ScalePixelSize { get; set; }
        [MaxLength(12)] // Number
        public string SourceToDetector { get; set; }
        [MaxLength(12)] // Number
        public string SupportToDetector { get; set; }
        [MaxLength(50)] 
        public string TubeType { get; set; }
        [MaxLength(5)]
        public string UseNewSlantAlgorithm { get; set; }
        [MaxLength(3)]
        public string ValidToProcess { get; set; }
        [MaxLength(8)]
        public string BreastSide { get; set; }
        [MaxLength(8)]
        public string ChestPosition { get; set; }
        [MaxLength(8)]
        public string PectoralPosition { get; set; }
        [MaxLength(3)]
        public string MammoView { get; set; }
        [MaxLength(10)]
        public string StudyDate { get; set; }
        [MaxLength(100)]
        public string OperatorName { get; set; }
        [MaxLength(10)]
        public string PatientDOB { get; set; }
        [MaxLength(3)]
        public string PatientAge { get; set; }
        [MaxLength(10)]
        public string PatientID { get; set; }
        [MaxLength(50)]
        public string DetectorID { get; set; }
        [MaxLength(50)]
        public string XraySystem { get; set; }
        [MaxLength(12)] // Number
        public string TargetPixelSizeMm { get; set; }
        [MaxLength(50)]
        public string NearestNeighborResample { get; set; }
        [MaxLength(12)] // Number
        public string ResizedPixelSizeMm { get; set; }
        [MaxLength(10)]
        public string PectoralSide { get; set; }
        [MaxLength(10)]
        public string PaddleType { get; set; }
        [MaxLength(12)] // Number
        public string ExposureMas { get; set; }
        [MaxLength(12)] // Number
        public string ExposureTimeMs { get; set; }
        [MaxLength(20)]
        public string TargetMaterial { get; set; }
        [MaxLength(20)]
        public string FilterMaterial { get; set; }
        [MaxLength(12)] // Number
        public string FilterThicknessMm { get; set; }
        [MaxLength(12)] // Number
        public string TubeVoltageKvp { get; set; }
        [MaxLength(12)] // Number
        public string CompressionPlateSlant { get; set; }
        [MaxLength(12)] // Number
        public string HVL_Mm { get; set; }
        [MaxLength(12)] // Number
        public string CompressionForceN { get; set; }
        [MaxLength(12)] // Number
        public string RecordedBreastThicknessMm { get; set; }
        [MaxLength(200)] 
        public string InnerBreastStatistics { get; set; }
        [MaxLength(12)] // Number
        public string muFatPerMm { get; set; }
        [MaxLength(12)] // Number
        public string MethodAllPlaneFit { get; set; }
        [MaxLength(200)]
        public string RejectingMethod1Reason { get; set; }
        [MaxLength(12)] // Number
        public string MethodFatPlaneFit { get; set; }
        [MaxLength(12)] // Number
        public string Calculated_Sigma { get; set; }
        [MaxLength(12)] // Number
        public string ComputedSlantAngle { get; set; }
        [MaxLength(12)] // Number
        public string ComputedSlantMm { get; set; }
        [MaxLength(12)] // Number
        public string ComputedBreastThickness { get; set; }
        [MaxLength(12)] // Number
        public string ScatterScaleFactor { get; set; }
        [MaxLength(20)] 
        public string Scatter { get; set; }
        [MaxLength(12)] // Number
        public string SegPhaseDE { get; set; }
        [MaxLength(12)] // Number
        public string SegPhaseOD { get; set; }
        [MaxLength(12)] // Number
        public string SegPhaseBE { get; set; }
        [MaxLength(12)] // Number
        public string SegPhasePA { get; set; }
        [MaxLength(12)] // Number
        public string SegPhaseBA { get; set; }
        [MaxLength(12)] // Number
        public string SegPhaseOA { get; set; }
        [MaxLength(12)] // Number
        public string SegPhaseUA { get; set; }
        [MaxLength(12)] // Number
        public string SegPhasePD { get; set; }
        [MaxLength(12)] // Number
        public string SegSphereDE { get; set; }
        [MaxLength(12)] // Number
        public string SegSphereOD { get; set; }
        [MaxLength(12)] // Number
        public string SegSphereBE { get; set; }
        [MaxLength(12)] // Number
        public string SegSpherePA { get; set; }
        [MaxLength(12)] // Number
        public string SegSphereBA { get; set; }
        [MaxLength(12)] // Number
        public string SegSphereOA { get; set; }
        [MaxLength(12)] // Number
        public string SegSphereUA { get; set; }
        [MaxLength(12)] // Number
        public string SegSpherePD { get; set; }
        [MaxLength(12)] // Number
        public string ContactAreaMm2 { get; set; }
        [MaxLength(12)] // Number
        public string CompressionPressureKPa { get; set; }
        [MaxLength(12)] // Number
        public string PFAT_Edge_Zone { get; set; }
        [MaxLength(12)] // Number
        public string HintRejectLevel { get; set; }
        [MaxLength(12)] // Number
        public string HintIgnoreLevel { get; set; }
        [MaxLength(12)] // Number
        public string EntranceDoseInmGy { get; set; }
        [MaxLength(12)] // Number
        public string EstimatedEntranceDoseInmGy { get; set; }
        public string Warning { get; set; }
        [MaxLength(12)] // Number
        public string GlandularityPercent { get; set; }
        [MaxLength(12)] // Number
        public string VolparaMeanGlandularDoseInmGy { get; set; }
        [MaxLength(12)] // Number
        public string FiftyPercentGlandularDoseInmGy { get; set; }
        [MaxLength(12)] // Number
        public string OrganDose { get; set; }
        [MaxLength(12)] // Number
        public string OrganDoseInmGy { get; set; }
        [MaxLength(200)] 
        public string CorrectionComplete { get; set; }
        [MaxLength(12)] // Number
        public string NippleConfidence { get; set; }
        [MaxLength(200)]
        public string NippleConfidenceMessage { get; set; }
        [MaxLength(3)]
        public string NippleInProfile { get; set; }
        [MaxLength(12)] // Number
        public string NippleDistanceFromPosteriorEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string NippleCenterDistanceFromPosteriorEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string MeanDenseThicknessInMm { get; set; }
        [MaxLength(12)] // Number
        public string MaximumDenseThicknessInMm { get; set; }
        [MaxLength(12)] // Number
        public string SDDenseThicknessInMm { get; set; }
        [MaxLength(12)] // Number
        public string MaximumDenseThicknessDistanceFromPosteriorEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string DensityMapAttenuatingPixelCount { get; set; }
        [MaxLength(12)] // Number
        public string MaximumPercentDensityIn1Cm2Area { get; set; }
        [MaxLength(12)] // Number
        public string MaximumDenseVolumeIn1Cm2AreaInCm3 { get; set; }
        [MaxLength(12)] // Number
        public string MaximumDensity1Cm2AreaDistanceFromPosteriorEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string DenseAreaPercent { get; set; }
        [MaxLength(12)] // Number
        public string AreaGreaterThan10mmDenseMm2 { get; set; }
        [MaxLength(12)] // Number
        public string HintVolumeCm3 { get; set; }
        [MaxLength(12)] // Number
        public string BreastVolumeCm3 { get; set; }
        [MaxLength(12)] // Number
        public string VolumetricBreastDensity { get; set; }
        [MaxLength(12)] // Number
        public string Out_BreastVolume { get; set; }
        [MaxLength(12)] // Number
        public string Out_FGTV { get; set; }
        [MaxLength(12)] // Number
        public string Out_Density { get; set; }
        public string Run_Information { get; set; }
        public string VolparaOkay { get; set; }

        // CC fields
        [MaxLength(20)]
        public string MedialSide { get; set; }
        [MaxLength(12)] // Number
        public string NippleDistanceFromMedialEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string NippleCenterDistanceFromMedialEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string CCPosteriorNippleLineLengthInMm { get; set; }
        [MaxLength(12)] // Number
        public string NippleMedialLateralDistanceInMm { get; set; }
        [MaxLength(12)] // Number
        public string NippleMedialLateralAngleInDegrees { get; set; }
        [MaxLength(12)] // Number
        public string BreastCenterToImageCenterDistanceInMm { get; set; }
        [MaxLength(12)] // Number
        public string BreastCenterDistanceFromMedialEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string BreastEdgeDistanceToPosteriorMedialCornerInMm { get; set; }
        [MaxLength(12)] // Number
        public string BreastEdgeDistanceToPosteriorLateralCornerInMm { get; set; }
        [MaxLength(3)]
        public string CleavageDetected { get; set; }
        [MaxLength(3)]
        public string ShoulderDetected { get; set; }
        [MaxLength(12)] // Number
        public string MaximumDenseThicknessDistanceFromMedialEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string MaximumDensity1Cm2AreaDistanceFromMedialEdgeInMm { get; set; }

        // MLO fields
        [MaxLength(12)] // Number
        public string PectoralAngleDegrees { get; set; }
        [MaxLength(12)] // Number
        public string PectoralAngleConfidence { get; set; }
        [MaxLength(12)] // Number
        public string NippleDistanceFromSuperiorEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string NippleCenterDistanceFromSuperiorEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string MLOPosteriorNippleLineLengthInMm { get; set; }
        [MaxLength(12)] // Number
        public string NippleLineLengthInMm { get; set; }
        [MaxLength(12)] // Number
        public string PNLToInferiorPectoralMuscleVerticalLengthInMm { get; set; }
        [MaxLength(3)]
        public string PectoralSkinFoldPresent { get; set; }
        [MaxLength(12)] // Number
        public string NippleToInferiorPectoralMuscleVerticalLengthInMm { get; set; }
        [MaxLength(12)] // Number
        public string SuperiorPectoralWidthInMm { get; set; }
        [MaxLength(12)] // Number
        public string PosteriorPectoralLengthInMm { get; set; }
        [MaxLength(20)]
        public string PectoralShape { get; set; }
        [MaxLength(12)] // Number
        public string ImfMaxDistanceMm { get; set; }
        [MaxLength(3)]
        public string InframammaryFoldVisible { get; set; }
        [MaxLength(12)] // Number
        public string InframammaryFoldAreaInMm2 { get; set; }
        [MaxLength(12)] // Number
        public string ImfAngleInDegrees { get; set; }
        [MaxLength(3)]
        public string ImfSkinFoldPresent { get; set; }
        [MaxLength(12)] // Number
        public string MaximumDenseThicknessDistanceFromSuperiorEdgeInMm { get; set; }
        [MaxLength(12)] // Number
        public string MaximumDensity1Cm2AreaDistanceFromSuperiorEdgeInMm { get; set; }

        public virtual Image Image { get; set; }

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; }
    }
}
