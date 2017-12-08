using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class ScreeningRecordV1_5_2
    {
        public int Id { get; set; }

        public int DataDate { get; set; }

        [MaxLength(30)]
        public string Version { get; set; }

        [MaxLength(10)]
        public string Detector { get; set; }

        [MaxLength(20)]
        public string XRaySystem { get; set; }

        [MaxLength(100)]
        public string ManufacturerModelName { get; set; }

        public DateTime? StudyDate { get; set; }

        public DateTime? AccessionDate { get; set; }

        public DateTime? ContentDate { get; set; }

        public int? PatientAge { get; set; }

        [MaxLength(15)]
        public string AccessionNumber { get; set; }

        [MaxLength(40)]
        public string StudyInstanceUID { get; set; }

        [MaxLength(40)]
        public string SeriesInstanceUID { get; set; }

        public int? InstanceNumber { get; set; }

        [MaxLength(5)]
        public string BreastSide { get; set; }

        [MaxLength(3)]
        public string MammoView { get; set;}

        [MaxLength(5)]
        public string ChestPosition { get; set; }

        public int? ExposureMas { get; set; }

        public int? ExposureTimeMs { get; set; }

        public int? TubeVoltageKvp { get; set; }

        public int? XRayTubeCurrent { get; set; }

        public double? DetectorTemperature { get; set; }

        [MaxLength(10)]
        public string TargetMaterial { get; set; }

        [MaxLength(10)]
        public string FilterMaterial { get; set; }

        public double? FilterThicknessMm { get; set; }

        public double? HVLMm { get; set; }

        public int? CompressionForceN { get; set; }

        public int? RecordedBreastThicknessMm { get; set; }

        public double? BreastVolumeCm3 { get; set; }

        public double? HintVolumeCm3 { get; set; }

        public double? VolumetricBreastDensity { get; set; }

        public double? CompressionPressureKPa { get; set; }

        public double? ContactAreasMm2 { get; set; }

        public double? DenseAreaPercent { get; set; }

        public double? AreaGreaterThan10mmDenseMm2 { get; set; }

        public double? ComputedSlantAngle { get; set; }

        public double? ComputerSlantMm { get; set; }

        public int? ComputedBreastThickness { get; set; }

        public int? PectoralAngleDegrees { get; set; }

        public int? PositionerPrimaryAngle { get; set; }

        public double? DetectorPrimaryAngle { get; set; }

        public double? DetectorSecondaryAngle { get; set; }

        public double? EntranceDoseInMgy { get; set; }

        public double? OrganDoseInMgy { get; set; }

        public double? GlandularityPercent { get; set;}

        public double? VolparaMeanGlandularDoseInMgy { get; set; }

        [MaxLength(50)]
        public string OperatorName { get; set; }

        [MaxLength(100)]
        public string RequestedProcedure { get; set;}

        [MaxLength(100)]
        public string ReferringPhysicianName { get; set; }

        [MaxLength(1000)]
        public string VolparaError { get; set; }

        [MaxLength(1000)]
        public string Warning { get; set; }

        [MaxLength(1000)]
        public string Special { get; set; }

        [MaxLength(3)]
        public string MammoImageType { get; set; }

        [MaxLength(100)]
        public string ImageQualityDiagnostics { get; set; }

        [MaxLength(3)]
        public string NippleInProfile { get; set; }

        public double? NippleAreaInMm2 { get; set; }

        public double? PercentNippleAreaAnteriorToSkinBoundary { get; set; }

        [MaxLength(10)]
        public string NippleCenterLocationRelativeToSkinLine { get; set; }

        public double? NippleToInferiorPectoralMuscleVerticalLengthInMm { get; set; }

        public double? PNLToInferiorPectoralMuscleVerticalLengthInMm { get; set; }

        public double? MLOPosterierNippleLineLengthInMm { get; set; }

        public double? NippleLineLengthInMm { get; set; }

        [MaxLength(3)]
        public string InframammaryFoldVisible { get; set; }

        public double? InframammaryFoldAreaInMm2 { get; set; }

        public double? InframammaryFoldDistanceFromPosteriorEdgeInMm { get; set; }

        public double? InframammaryFoldDistanceFromSuperiorEdgeInMm { get; set; }

        public double? SuperiorPectoralWidthInMm { get; set; }

        public double? PosteriorPectoralLengthInMm { get; set; }

        [MaxLength(10)]
        public string PectoralShape { get; set; }

        public double? NippleDistanceFromSuperiorEdgeInMm { get; set; }

        public double? NippleDistanceFromPosteriorEdgeInMm { get; set; }

        public double? CCPosteriorNippleLineLengthInMm { get; set; }

        public double? NippleMedialLateralDistanceInMm { get; set; }

        public double? NippleMedialLateralAngleInDegrees { get; set; }

        public double? BreastCenterToImageCenterDistanceInMm { get; set; }

        public double? NippleDistanceFromMedialEdgeInMm { get; set; }

        public double? BreastEdgeDistanceToPosteriorMedialCornerInMm { get; set; }

        public double? BreastEdgeDistanceToPosteriorLateralCornerInMm { get; set; }

        [MaxLength(3)]
        public string ShoulderDetected { get; set; }

        [MaxLength(3)]
        public string CleavageDetected { get; set; }

        public double? MeanDenseThicknessInMm { get; set; }

        public double? MaximumDenseThicknessInMm { get; set; }

        public double? SDDenseThicknessInMm { get; set; }

        public double? MaximumPercentDensityIn1Cm2Area { get; set; }

        public double? MaximumDenseVolumeIn1Cm2AreaInCm3 { get; set; }

        public double? MaximumDensity1Cm2AreaDistanceFromSuperiorEdgeInMm { get; set; }

        public double? MaximumDensity1Cm2AreaDistanceFromMedialEdgeInMm { get; set; }

        public double? MaximumDensity1Cm2AreaDistanceFromPosteriorEdgeInMm { get; set; }

        public double? DoseCalibrationError { get; set; }

        public double? MeasuredHVLInMmAl { get; set; }

        public double? MeasuredEntranceDoseInMgy { get; set; }

        public DateTime? DoseCalibrationUsedDateTime { get; set; }

        public double? DoseCalculationBasis { get; set; }

       
        public virtual Image Image { get; set; }

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; }
    }
}
