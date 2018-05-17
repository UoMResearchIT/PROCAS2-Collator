using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace PROCAS2.Models.ServiceBusMessages
{
    public class ScreeningXlsMessage
    {
        // Fields common to both CC and MLO MammoTypes
        public string MammoImageType { get; set; }
        public string RequestedProcedure { get; set; }
        public string Folder { get; set; }
        public string Timestamp { get; set; }
        public string OSName { get; set; }
        public string OSVersion { get; set; }
        public string CurrentCulture { get; set; }
        public string InstalledUICulture { get; set; }
        public string WriteOutDisplayImage { get; set; }
        public string VolparaVersion { get; set; }
        public string DICOMTAGManufacturer { get; set; }
        public string DICOMTAGDeviceSerialNumber { get; set; }
        public string DICOMTagDetector_ID { get; set; }
        public string MaxAllowedKVP { get; set; }
        public string BTSF { get; set; }
        public string BTSTF { get; set; }
        public string DetectorType { get; set; }
        public string DoFlatFieldCorrection { get; set; }
        public string FSensitivity { get; set; }
        public string Gain { get; set; }
        public string NativePixelSize { get; set; }
        public string Offset { get; set; }
        public string ScalePixelSize { get; set; }
        public string SourceToDetector { get; set; }
        public string SupportToDetector { get; set; }
        public string TubeType { get; set; }
        public string UseNewSlantAlgorithm { get; set; }
        public string ValidToProcess { get; set; }
        public string BreastSide { get; set; }
        public string ChestPosition { get; set; }
        public string PectoralPosition { get; set; }
        public string MammoView { get; set; }
        public string StudyDate { get; set; }
        public string OperatorName { get; set; }
        public string PatientDOB { get; set; }
        public string PatientAge { get; set; }
        public string PatientID { get; set; }
        public string DetectorID { get; set; }
        public string XraySystem { get; set; }
        public string TargetPixelSizeMm { get; set; }
        public string NearestNeighborResample { get; set; }
        public string ResizedPixelSizeMm { get; set; }
        public string PectoralSide { get; set; }
        public string PaddleType { get; set; }
        public string ExposureMas { get; set; }
        public string ExposureTimeMs { get; set; }
        public string TargetMaterial { get; set; }
        public string FilterMaterial { get; set; }
        public string FilterThicknessMm { get; set; }
        public string TubeVoltageKvp { get; set; }
        public string CompressionPlateSlant { get; set; }
        public string HVL_Mm { get; set; }
        public string CompressionForceN { get; set; }
        public string RecordedBreastThicknessMm { get; set; }
        public string InnerBreastStatistics { get; set; }
        public string muFatPerMm { get; set; }
        public string MethodAllPlaneFit { get; set; }
        public string RejectingMethod1Reason { get; set; }
        public string MethodFatPlaneFit { get; set; }
        public string Calculated_Sigma { get; set; }
        public string ComputedSlantAngle { get; set; }
        public string ComputedSlantMm { get; set; }
        public string ComputedBreastThickness { get; set; }
        public string ScatterScaleFactor { get; set; }
        public string Scatter { get; set; }
        public string SegPhaseDE { get; set; }
        public string SegPhaseOD { get; set; }
        public string SegPhaseBE { get; set; }
        public string SegPhasePA { get; set; }
        public string SegPhaseBA { get; set; }
        public string SegPhaseOA { get; set; }
        public string SegPhaseUA { get; set; }
        public string SegPhasePD { get; set; }
        public string SegSphereDE { get; set; }
        public string SegSphereOD { get; set; }
        public string SegSphereBE { get; set; }
        public string SegSpherePA { get; set; }
        public string SegSphereBA { get; set; }
        public string SegSphereOA { get; set; }
        public string SegSphereUA { get; set; }
        public string SegSpherePD { get; set; }
        public string ContactAreaMm2 { get; set; }
        public string CompressionPressureKPa { get; set; }
        public string PFAT_Edge_Zone { get; set; }
        public string HintRejectLevel { get; set; }
        public string HintIgnoreLevel { get; set; }
        public string EntranceDoseInmGy { get; set; }
        public string EstimatedEntranceDoseInmGy { get; set; }
        public string Warning { get; set; }
        public string GlandularityPercent { get; set; }
        public string VolparaMeanGlandularDoseInmGy { get; set; }
        public string FiftyPercentGlandularDoseInmGy { get; set; }
        public string OrganDose { get; set; }
        public string OrganDoseInmGy { get; set; }
        public string CorrectionComplete { get; set; }
        public string NippleConfidence { get; set; }
        public string NippleConfidenceMessage { get; set; }
        public string NippleInProfile { get; set; }
        public string NippleDistanceFromPosteriorEdgeInMm { get; set; }
        public string NippleCenterDistanceFromPosteriorEdgeInMm { get; set; }
        public string MeanDenseThicknessInMm { get; set; }
        public string MaximumDenseThicknessInMm { get; set; }
        public string SDDenseThicknessInMm { get; set; }
        public string MaximumDenseThicknessDistanceFromPosteriorEdgeInMm { get; set; }
        public string DensityMapAttenuatingPixelCount { get; set; }
        public string MaximumPercentDensityIn1Cm2Area { get; set; }
        public string MaximumDenseVolumeIn1Cm2AreaInCm3 { get; set; }
        public string MaximumDensity1Cm2AreaDistanceFromPosteriorEdgeInMm { get; set; }
        public string DenseAreaPercent { get; set; }
        public string AreaGreaterThan10mmDenseMm2 { get; set; }
        public string HintVolumeCm3 { get; set; }
        public string BreastVolumeCm3 { get; set; }
        public string VolumetricBreastDensity { get; set; }
        public string Out_BreastVolume { get; set; }
        public string Out_FGTV { get; set; }
        public string Out_Density { get; set; }
        public string Run_Information { get; set; }
        public string VolparaOkay { get; set; }

        // CC fields
        public string MedialSide { get; set; }
        public string NippleDistanceFromMedialEdgeInMm { get; set; }
        public string NippleCenterDistanceFromMedialEdgeInMm { get; set; }
        public string CCPosteriorNippleLineLengthInMm { get; set; }
        public string NippleMedialLateralDistanceInMm { get; set; }
        public string NippleMedialLateralAngleInDegrees { get; set; }
        public string BreastCenterToImageCenterDistanceInMm { get; set; }
        public string BreastCenterDistanceFromMedialEdgeInMm { get; set; }
        public string BreastEdgeDistanceToPosteriorMedialCornerInMm { get; set; }
        public string BreastEdgeDistanceToPosteriorLateralCornerInMm { get; set; }
        public string CleavageDetected { get; set; }
        public string ShoulderDetected { get; set; }
        public string MaximumDenseThicknessDistanceFromMedialEdgeInMm { get; set; }
        public string MaximumDensity1Cm2AreaDistanceFromMedialEdgeInMm { get; set; }

        // MLO fields
        public string PectoralAngleDegrees { get; set; }
        public string PectoralAngleConfidence { get; set; }
        public string NippleDistanceFromSuperiorEdgeInMm { get; set; }
        public string NippleCenterDistanceFromSuperiorEdgeInMm { get; set; }
        public string MLOPosteriorNippleLineLengthInMm { get; set; }
        public string NippleLineLengthInMm { get; set; }
        public string PNLToInferiorPectoralMuscleVerticalLengthInMm { get; set; }
        public string PectoralSkinFoldPresent { get; set; }
        public string NippleToInferiorPectoralMuscleVerticalLengthInMm { get; set; }
        public string SuperiorPectoralWidthInMm { get; set; }
        public string PosteriorPectoralLengthInMm { get; set; }
        public string PectoralShape { get; set; }
        public string ImfMaxDistanceMm { get; set; }
        public string InframammaryFoldVisible { get; set; }
        public string InframammaryFoldAreaInMm2 { get; set; }
        public string ImfAngleInDegrees { get; set; }
        public string ImfSkinFoldPresent { get; set; }
        public string MaximumDenseThicknessDistanceFromSuperiorEdgeInMm { get; set; }
        public string MaximumDensity1Cm2AreaDistanceFromSuperiorEdgeInMm { get; set; }
    }
}
