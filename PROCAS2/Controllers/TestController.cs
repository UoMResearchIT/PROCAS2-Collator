using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Models.TestModels;
using PROCAS2.Services.App;
using PROCAS2.Data.Entities;
using PROCAS2.Data;
using PROCAS2.CustomAttributes;
using PROCAS2.Services.Utility;

namespace PROCAS2.Controllers
{
    /// <summary>
    /// Used for spoofing ServiceBus messages on the PROCAS2 test system. Should not be available on production!
    /// All error messages hardcoded, because this is supposed to be a test-only thing, and I am in a rush. :-)
    /// </summary>
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    [RedirectIfNotTest]
    public class TestController : Controller
    {
        private IWebJobParticipantService _participantService;
        private IGenericRepository<Participant> _participantRepo;
        private ICRAService _craService;
        private IVolparaService _volparaService;

        public TestController(IWebJobParticipantService participantService,
                                IGenericRepository<Participant> participantRepo,
                                ICRAService craService,
                                IVolparaService volparaService)
        {
            _participantService = participantService;
            _participantRepo = participantRepo;
            _craService = craService;
            _volparaService = volparaService;
        }

        [HttpGet]
        public ActionResult ReceiveConsent()
        {
            ReceiveConsentViewModel model = new ReceiveConsentViewModel();
            return View("ReceiveConsent", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReceiveConsent(ReceiveConsentViewModel model)
        {
            if (ModelState.IsValid)
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == model.NHSNumber).FirstOrDefault();
                if (participant != null)
                {
                    if (_participantService.SetConsentFlag(participant.HashedNHSNumber) == true)
                    {
                        return RedirectToAction("Index", "Participant");
                    }
                    else
                    {
                        ModelState.AddModelError("NHSNumber", "Error setting consent");
                    }
                }
                else
                {
                    ModelState.AddModelError("NHSNumber", "Participant does not exist");
                }
            }

            
            return View("ReceiveConsent", model);
        }


        [HttpGet]
        public ActionResult ReceiveLetter()
        {
            ReceiveLetterViewModel model = new ReceiveLetterViewModel();
            return View("ReceiveLetter", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReceiveLetter(ReceiveLetterViewModel model)
        {
            string _exampleMessage = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||PATIENTID||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|1|TX|100^Risk Assessment Summary^CRA||**********BREAST CANCER RISK ASSESSMENT SUMMARY**********~Risk Assessment results are provided by CRA Health, Inc. ~Your patient completed a breast cancer risk assessment survey as part of her breast health screening.Based on the information she provided, her calculated risk of developing breast cancer is at a higher than average lifetime risk and / or hereditary predisposition to cancer. ~High risk patients, especially those with dense breasts, will benefit from additional clinical management and annual Breast MRI. ~Estimated Breast Cancer Risk Calculations: ~Risk BRCA1 / 2 Mutation: 1.5 % ~Lifetime Breast Cancer Risk: 35.4 % ~*ACS Guidelines: Patients at high risk will have a >= 10 % risk of BRCA mutation and/ or >= 20 % lifetime risk of breast cancer ~*This analysis is only as accurate as the data entered or provided by the patient. ~Date of the Survey Calculations: Oct 30 2017 9:50PM~||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire.Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer.All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent.Key criteria for increased risk are: ~-Personal or family history of breast and / or ovarian cancer~-History of abnormal breast pathology~-Personal history of chest radiation~-Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|3|TX|120^Screening Mammography Provider Letter^CRA||Your patient completed a cancer risk assessment questionnaire during a visit to our screening facility.She provided the following information regarding their personal and family history: ~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~The preliminary information your patient provided shows that she meets the criteria of the American Cancer Society Guidelines1 and the National Cancer Center Network(NCCN)2 Guidelines for screening MRI of the breast.Both guidelines recommend MRI of the breast if the patient has a lifetime risk of breast cancer of 20 % or greater based on the Tyrer Cuzick, BRCAPRO or Claus Model.Your patient's risk is elevated by an accepted model:~ - Tyrer Cuzick (v8) 35.4% lifetime risk~Thus, she is considered at significantly increased risk of developing breast cancer as compared to other women her age. MRI would increase the chance of finding breast cancer at an earlier, more treatable stage. We recommend that you order this study on an annual basis. This would be in addition to her routine annual screening mammography.~If you agree, we suggest you order a breast screening MRI. The reason for the test should be listed as 'Lifetime Risk of breast cancer of 20 % or greater by an accepted model'.~We have suggested that the patient talk with you about this if there are any concerns.~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available. Thank you for letting us participate in the care of your patients.~||||||F
OBX|4|NM|200^BRCA 1/2 Mutation Risk Score^CRA|BRCAPRO|0.91|%^Percent^ISO|||||F
OBX|5|NM|200^BRCA 1/2 Mutation Risk Score^CRA|TC6|1.4|%^Percent^ISO|||||F
OBX|6|NM|200^BRCA 1/2 Mutation Risk Score^CRA|TC7|0.91|%^Percent^ISO|||||F
OBX|7|NM|200^BRCA 1/2 Mutation Risk Score^CRA|TC8|0.91|%^Percent^ISO|||||F
OBX|8|NM|200^BRCA 1/2 Mutation Risk Score^CRA|Myriad|1.49|%^Percent^ISO|||||F
OBX|9|NM|210^5-Year Breast Cancer Risk Score^CRA|BRCAPRO|1|%^Percent^ISO|||||F
OBX|10|NM|210^5-Year Breast Cancer Risk Score^CRA|TC6|3.42|%^Percent^ISO|||||F
OBX|11|NM|210^5-Year Breast Cancer Risk Score^CRA|TC7|3.51|%^Percent^ISO|||||F
OBX|12|NM|210^5-Year Breast Cancer Risk Score^CRA|TC8|3.51|%^Percent^ISO|||||F
OBX|13|NM|210^5-Year Breast Cancer Risk Score^CRA|Claus|2.08|%^Percent^ISO|||||F
OBX|14|NM|210^5-Year Breast Cancer Risk Score^CRA|Gail|3.01|%^Percent^ISO|||||F
OBX|16|NM|215^10-Year Breast Cancer Risk Score^CRA|TC8|7.62|%^Percent^ISO|||||F
OBX|15|NM|220^Lifetime Breast Cancer Risk Score^CRA|BRCAPRO|10.48|%^Percent^ISO|||||F
OBX|16|NM|220^Lifetime Breast Cancer Risk Score^CRA|TC6|27.67|%^Percent^ISO|||||F
OBX|17|NM|220^Lifetime Breast Cancer Risk Score^CRA|TC7|35.44|%^Percent^ISO|||||F
OBX|18|NM|220^Lifetime Breast Cancer Risk Score^CRA|TC8|35.44|%^Percent^ISO|||||F
OBX|19|NM|220^Lifetime Breast Cancer Risk Score^CRA|Claus|15.55|%^Percent^ISO|||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|21|NM|230^5-Year Ovarian Cancer Risk Score^CRA|BRCAPRO|0.09|%^Percent^ISO|||||F
OBX|22|NM|240^Lifetime Ovarian Cancer Risk Score^CRA|BRCAPRO|1.38|%^Percent^ISO|||||F
OBX|23|NM|300^HNPCC Mutation Risk Score^CRA|MMRPRO|0.09|%^Percent^ISO|||||F
OBX|24|NM|300^HNPCC Mutation Risk Score^CRA|PREMM2|0.99|%^Percent^ISO|||||F
OBX|25|NM|310^5-Year Colorectal Cancer Risk Score^CRA|MMRPRO|0.09|%^Percent^ISO|||||F
OBX|26|NM|320^Lifetime Colorectal Cancer Risk Score^CRA|MMRPRO|2.91|%^Percent^ISO|||||F
OBX|27|NM|330^5-Year Endometrial Cancer Risk Score^CRA|MMRPRO|0.09|%^Percent^ISO|||||F
OBX|28|NM|340^Lifetime Endometrial Cancer Risk Score^CRA|MMRPRO|1.82|%^Percent^ISO|||||F
OBX|29|CE|400^Lifetime Breast Cancer Risk Category^CRA||HIGH||||||F
OBX|30|CE|410^HBOC Risk Category^CRA||AVERAGE||||||F
OBX|31|CE|420^HNPCC Risk Category^CRA||AVERAGE||||||F
OBX|34|CE|430^Genetic Testing Recommendation^CRA||NO||||||F
OBX|35|CE|500^Family History^CRA||1^SELF&self^F^47^&Other^50~2^NMTH&mother^F^83^&Breast Cancer^60~2^NMTH&mother^F^83^&Breast Cancer^63~3^NFTH&father^M^83^&Other^80~4^MGRMTH&maternal grandmother^F^80^&Breast Cancer^70~5^MGRFTH&maternal grandfather^M^82^&Prostate Cancer^80~7^PGRFTH&paternal grandfather^M^49^&Other^48~13^MAUNT&maternal aunt^F^75^&Breast Cancer^75~14^MAUNT&maternal aunt^F^75^&Breast Cancer^65~17^MUNCLE&maternal uncle^M^74^&Unknown^74~19^PAUNT&paternal aunt^F^75^&Uterine Cancer^74~24^MCOUSN&maternal cousion^F^^&Breast Cancer^65~24^MCOUSN&maternal cousion^F^^&Breast Cancer^68||||||F
OBX|46|CE|510^Reported Family Genetic Testing Results^CRA||1^SELF&self^F^47^&BRCA1^Pathogenic~2^NMTH&mother^F^83^&BRCA1^Pathogenic~4^MGRMTH&maternal grandmother^F^80^&BRCA1^Pathogenic~5^MGRFTH&maternal grandfather^M^82^&BRCA1^Negative~5^MGRFTH&maternal grandfather^M^82^&BRCA2^Negative~7^PGRFTH&paternal grandfather^M^49^&BRCA1^Unknown~7^PGRFTH&paternal grandfather^M^49^&BRCA2^Unknown~8^NSIS&sister^F^55^&BRCA1^Pathogenic~10^NBRO&brother^M^47^&BRCA1^Negative~10^NBRO&brother^M^47^&BRCA2^Negative~12^MAUNT&maternal aunt^F^78^&BRCA1^Pathogenic~13^MAUNT&maternal aunt^F^75^&BRCA1^Pathogenic~14^MAUNT&maternal aunt^F^75^&BRCA1^Pathogenic||||||F
OBX|37|ED|900^Risk Assessment Document^CRA|Survey Summary|^^PDF^base64^......||||||F
OBX|38|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|5|TX|1000.AdditionalQuestions||Yes||||||F
OBX|6|TX|1000.ageFirstChildBorn||25||||||F
OBX|7|TX|1000.AlcoholHowMuch||5||||||F
OBX|8|TX|1000.AlcoholYesNo||Yes||||||F
OBX|9|TX|1000.apptID||200||||||F
OBX|10|TX|1000.BiopsyIncreasedRisk||Yes||||||F
OBX|11|TX|1000.BiopsyResult||LCIS||||||F
OBX|12|TX|1000.birthControlContinuously||Yes||||||F
OBX|13|TX|1000.birthControlUse||Yes, in the past||||||F
OBX|14|TX|1000.birthControlYears||5||||||F
OBX|15|TX|1000.BMI||2||||||F
OBX|16|TX|1000.BMICategory||Severely underweight||||||F
OBX|17|TX|1000.bothOvariesRemoved||No||||||F
OBX|18|TX|1000.BRCATestingResult1||BRCA1||||||F
OBX|19|TX|1000.BRCATestingResult10||Negative||||||F
OBX|20|TX|1000.BRCATestingResult12||BRCA1||||||F
OBX|21|TX|1000.BRCATestingResult13||BRCA1||||||F
OBX|22|TX|1000.BRCATestingResult14||BRCA1||||||F
OBX|23|TX|1000.BRCATestingResult2||BRCA1||||||F
OBX|24|TX|1000.BRCATestingResult4||BRCA1||||||F
OBX|25|TX|1000.BRCATestingResult5||Negative||||||F
OBX|26|TX|1000.BRCATestingResult7||Unknown||||||F
OBX|27|TX|1000.BRCATestingResult8||BRCA1||||||F
OBX|28|TX|1000.BRCATestingResultSelf||BRCA1||||||F
OBX|29|TX|1000.BRCATestingResultYour1stOf3MaternalAunts||BRCA1||||||F
OBX|30|TX|1000.BRCATestingResultYour2ndOf3MaternalAunts||BRCA1||||||F
OBX|31|TX|1000.BRCATestingResultYour3rdOf3MaternalAunts||BRCA1||||||F
OBX|32|TX|1000.BRCATestingResultYourBrother||Negative||||||F
OBX|33|TX|1000.BRCATestingResultYourGrandfatherOnYourFatherSSide||Unknown||||||F
OBX|34|TX|1000.BRCATestingResultYourGrandfatherOnYourMotherSSide||Negative||||||F
OBX|35|TX|1000.BRCATestingResultYourGrandmotherOnYourMotherSSide||BRCA1||||||F
OBX|36|TX|1000.BRCATestingResultYourMother||BRCA1||||||F
OBX|37|TX|1000.BRCATestingResultYourSister||BRCA1||||||F
OBX|38|TX|1000.BreastCancer||Yes||||||F
OBX|39|TX|1000.breastImplants||Yes||||||F
OBX|40|TX|1000.breastImplantsSide||Both||||||F
OBX|41|TX|1000.breastReduction||No||||||F
OBX|42|TX|1000.ColonOrRectalCancer||No||||||F
OBX|43|TX|1000.consentYesNo||Yes||||||F
OBX|44|TX|1000.CreateBilateral2||No||||||F
OBX|45|TX|1000.CreateBilateral24||No||||||F
OBX|46|TX|1000.CurrentBilateral13||No||||||F
OBX|47|TX|1000.CurrentBilateral14||Not sure||||||F
OBX|48|TX|1000.CurrentBilateral2||Yes||||||F
OBX|49|TX|1000.CurrentBilateral24||Yes||||||F
OBX|50|TX|1000.CurrentBilateral4||No||||||F
OBX|51|TX|1000.DoYouHaveOrHaveYouEverHadCancer||No||||||F
OBX|52|TX|1000.emailAddress||test@hughesriskapps.com||||||F
OBX|53|TX|1000.EverBreastFeed||Yes||||||F
OBX|54|TX|1000.ExcerciseHrs||2||||||F
OBX|55|TX|1000.ExcerciseMins||30||||||F
OBX|56|TX|1000.ExcerciseYesNo||Yes||||||F
OBX|57|TX|1000.hadBreastBiopsy||Yes||||||F
OBX|58|TX|1000.HadGeneticTestingSelf||Yes||||||F
OBX|59|TX|1000.HadGeneticTestingYour1stOf2PaternalUncles||No||||||F
OBX|60|TX|1000.HadGeneticTestingYour1stOf3MaternalAunts||Yes||||||F
OBX|61|TX|1000.HadGeneticTestingYour1stOf3PaternalAunts||No||||||F
OBX|62|TX|1000.HadGeneticTestingYour1stOf4MaternalUncles||No||||||F
OBX|63|TX|1000.HadGeneticTestingYour2ndOf2PaternalUncles||No||||||F
OBX|64|TX|1000.HadGeneticTestingYour2ndOf3MaternalAunts||Yes||||||F
OBX|65|TX|1000.HadGeneticTestingYour2ndOf3PaternalAunts||No||||||F
OBX|66|TX|1000.HadGeneticTestingYour2ndOf4MaternalUncles||No||||||F
OBX|67|TX|1000.HadGeneticTestingYour3rdOf3MaternalAunts||Yes||||||F
OBX|68|TX|1000.HadGeneticTestingYour3rdOf3PaternalAunts||No||||||F
OBX|69|TX|1000.HadGeneticTestingYour3rdOf4MaternalUncles||No||||||F
OBX|70|TX|1000.HadGeneticTestingYour4thOf4MaternalUncles||No||||||F
OBX|71|TX|1000.HadGeneticTestingYourBrother||Yes||||||F
OBX|72|TX|1000.HadGeneticTestingYourFather||No||||||F
OBX|73|TX|1000.HadGeneticTestingYourGrandfatherOnYourFatherSSide||Yes||||||F
OBX|74|TX|1000.HadGeneticTestingYourGrandfatherOnYourMotherSSide||Yes||||||F
OBX|75|TX|1000.HadGeneticTestingYourGrandmotherOnYourFatherSSide||No||||||F
OBX|76|TX|1000.HadGeneticTestingYourGrandmotherOnYourMotherSSide||Yes||||||F
OBX|77|TX|1000.HadGeneticTestingYourHalfSister||No||||||F
OBX|78|TX|1000.HadGeneticTestingYourMother||Yes||||||F
OBX|79|TX|1000.HadGeneticTestingYourSister||Yes||||||F
OBX|80|TX|1000.hadHysterectomy||No||||||F
OBX|81|TX|1000.HalfSibRelativeID9SameDad||Yes||||||F
OBX|82|TX|1000.HalfSibRelativeID9SameMom||No||||||F
OBX|83|TX|1000.HasDiabetes||Yes, Type II diabetes||||||F
OBX|84|TX|1000.hasSmoked||Yes, in the past||||||F
OBX|85|TX|1000.heightFeetInches||55-55||||||F
OBX|86|TX|1000.heightInches||715||||||F
OBX|87|TX|1000.HeightPreferredUnits||Standard||||||F
OBX|88|TX|1000.hormoneUse||Yes, currently||||||F
OBX|89|TX|1000.HRTHowLong||10||||||F
OBX|90|TX|1000.HRTType||Combined oestrogen and progesterone (e.g. aaa, bbb, ccc, ddd, eee, fff, ggg, hhh?)||||||F
OBX|91|TX|1000.isAshkenazi||Yes||||||F
OBX|92|TX|1000.IUDWithHormones||No, never||||||F
OBX|93|TX|1000.KnowFamilyHistory||Yes||||||F
OBX|94|TX|1000.MaternalMaternalCousinFemale||Yes||||||F
OBX|95|TX|1000.MenopausalStatus||Pre-menopausal||||||F
OBX|96|TX|1000.MonthsBreastFedChild1||3||||||F
OBX|97|TX|1000.MonthsBreastFedChild2||3||||||F
OBX|98|TX|1000.Niece||No||||||F
OBX|99|TX|1000.nipplePiercing||Yes||||||F
OBX|100|TX|1000.nipplePiercingSide||Both||||||F
OBX|101|TX|1000.NumCigarettes||4||||||F
OBX|102|TX|1000.OCType||Combined pill (oestrogen and progesterone) e.g. Microgynon, Cilest, Marvelon||||||F
OBX|103|TX|1000.Other||No||||||F
OBX|104|TX|1000.OtherBreastSurgery||Yes||||||F
OBX|105|TX|1000.otherBreastSurgeryYesNo||No||||||F
OBX|106|TX|1000.OvarianCancer||No||||||F
OBX|107|TX|1000.OvariesRemoved||No||||||F
OBX|108|TX|1000.PancreaticCancer||No||||||F
OBX|109|TX|1000.PaternalPaternalCousinFemale||No||||||F
OBX|110|TX|1000.PolycysticOvaries||No||||||F
OBX|111|TX|1000.ProstateCancer||No||||||F
OBX|112|TX|1000.racialBackground||White||||||F
OBX|113|TX|1000.RelativeCancer10||No||||||F
OBX|114|TX|1000.RelativeCancer12||No||||||F
OBX|115|TX|1000.RelativeCancer13||Yes||||||F
OBX|116|TX|1000.RelativeCancer14||Yes||||||F
OBX|117|TX|1000.RelativeCancer15||No||||||F
OBX|118|TX|1000.RelativeCancer16||No||||||F
OBX|119|TX|1000.RelativeCancer17||Yes||||||F
OBX|120|TX|1000.RelativeCancer18||No||||||F
OBX|121|TX|1000.RelativeCancer19||Yes||||||F
OBX|122|TX|1000.RelativeCancer2||Yes||||||F
OBX|123|TX|1000.RelativeCancer20||No||||||F
OBX|124|TX|1000.RelativeCancer21||Not sure||||||F
OBX|125|TX|1000.RelativeCancer22||No||||||F
OBX|126|TX|1000.RelativeCancer23||Not sure||||||F
OBX|127|TX|1000.RelativeCancer3||Yes||||||F
OBX|128|TX|1000.RelativeCancer4||Yes||||||F
OBX|129|TX|1000.RelativeCancer5||Yes||||||F
OBX|130|TX|1000.RelativeCancer6||No||||||F
OBX|131|TX|1000.RelativeCancer7||Yes||||||F
OBX|132|TX|1000.RelativeCancer8||No||||||F
OBX|133|TX|1000.RelativeCancer9||No||||||F
OBX|134|TX|1000.RelativeHadGeneticTesting||Yes||||||F
OBX|135|TX|1000.RelativeTwin||No||||||F
OBX|136|TX|1000.RelativeTwin10||No||||||F
OBX|137|TX|1000.RelativeTwin8||No||||||F
OBX|138|TX|1000.Smoking||Yes, in the past||||||F
OBX|139|TX|1000.SmokingHowLong||8||||||F
OBX|140|TX|1000.startedMenstruating||11||||||F
OBX|141|TX|1000.surveyEnd||20180514155234||||||F
OBX|142|TX|1000.takenAromataseInhibitor||No, never||||||F
OBX|143|TX|1000.takenRaloxifene||No, never||||||F
OBX|144|TX|1000.takenTamoxifen||No, never||||||F
OBX|145|TX|1000.timesPregnant||2||||||F
OBX|146|TX|1000.UKClothingSize||8||||||F
OBX|147|TX|1000.Unknown||No||||||F
OBX|148|TX|1000.UterineCancer||No||||||F
OBX|149|TX|1000.WeightAt20||1390||||||F
OBX|150|TX|1000.weightPounds||1485||||||F
OBX|151|TX|1000.WeightPreferredUnits||Standard||||||F";

            

 
            if (ModelState.IsValid)
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == model.NHSNumber).FirstOrDefault();
                if (participant != null)
                {
                    _exampleMessage = _exampleMessage.Replace("PATIENTID", participant.HashedNHSNumber);
                    List<string> results = _craService.ProcessQuestionnaire(_exampleMessage);
                    if (results.Count == 0)
                    {
                        return RedirectToAction("Details", "Participant", new { participantId = model.NHSNumber });
                    }
                    else
                    {
                        foreach (string result in results)
                        {
                            ModelState.AddModelError("NHSNumber", result);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("NHSNumber", "Participant does not exist");
                }
            }


            return View("ReceiveLetter", model);
        }


        [HttpGet]
        public ActionResult ReceiveVolpara()
        {
            ReceiveVolparaViewModel model = new ReceiveVolparaViewModel();
            return View("ReceiveVolpara", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReceiveVolpara(ReceiveVolparaViewModel model)
        {
            string _exampleMessage = @"{
  'FileDescriptor': 'VolparaEnterprise Study Result (4 images)',
  'StudyResultCreationTimeUtc': '2018-04-26T05:22:18.1834989Z',
  'JsonVersion': 1,
  'ScorecardResults': {
    'LeftBreastFindings': {
      'FibroglandularTissueVolume': 49.4,
      'BreastVolume': 380.0,
      'VolumetricBreastDensity': 13.0,
      'JsonVersion': 1
    },
    'RightBreastFindings': {
      'FibroglandularTissueVolume': 48.8,
      'BreastVolume': 325.3,
      'VolumetricBreastDensity': 15.0,
      'JsonVersion': 1
    },
    'VolparaDensityPercentageUsingMaximumBreast': 14.0,
    'VolparaDensityPercentageUsingBreastAverage': 14.0,
    'VolparaDensityGrade4ThEdition': 3,
    'VolparaDensityGrade5ThEdition': 'c',
    'VolparaDensityGrade5ThEditionUsingBreastAverage': 'c',
    'DensityImagesUsedForLccLmloRccRmlo': [
      1,
      2,
      0,
      3
    ],
    'DensityOutliers': [],
    'AverageBreastVolume': 352.7,
    'AverageAppliedPressure': 12.9,
    'AverageAppliedForce': 80.0,
    'AverageManufacturerDosePerImage': 2.9,
    'AverageVolparaDosePerImage': 3.2,
    'LeftBreastTotalDose': 6.6,
    'RightBreastTotalDose': 6.3,
    'JsonVersion': 1
  },
  'VolparaServerScorecardResults': {
    'LeftBreastFindings': {
      'FibroglandularTissueVolume': 49.4,
      'BreastVolume': 380.0,
      'VolumetricBreastDensity': 13.0,
      'JsonVersion': 1
    },
    'RightBreastFindings': {
      'FibroglandularTissueVolume': 48.8,
      'BreastVolume': 325.3,
      'VolumetricBreastDensity': 15.0,
      'JsonVersion': 1
    },
    'VolparaDensityPercentageUsingMaximumBreast': 14.0,
    'VolparaDensityPercentageUsingBreastAverage': 14.0,
    'VolparaDensityGrade4ThEdition': 3,
    'VolparaDensityGrade5ThEdition': 'c',
    'VolparaDensityGrade5ThEditionUsingBreastAverage': 'c',
    'DensityImagesUsedForLccLmloRccRmlo': [
      1,
      2,
      0,
      3
    ],
    'DensityOutliers': [],
    'AverageBreastVolume': 352.7,
    'AverageAppliedPressure': 12.9,
    'AverageAppliedForce': 80.0,
    'AverageManufacturerDosePerImage': 2.9,
    'AverageVolparaDosePerImage': 3.2,
    'LeftBreastTotalDose': null,
    'RightBreastTotalDose': null,
    'JsonVersion': 1
  },
  'AnalyticsResults': null,
  'VolparaPgmiResults': null,
  'Configuration': {
    'StudyImageProcessingMode': 'MixedPreferMammo',
    'VolparaServerStudyImageProcessingMode': 'Mixed',
    'AbDensityGradeBoundary': 3.5,
    'BcDensityGradeBoundary': 7.5,
    'CdDensityGradeBoundary': 15.5,
    'JsonVersion': 1
  },
  'ImageResults': [
    {
      'JsonVersion': 1,
      'SourceImage': {
        'DicomImageFilePath': '0001_GE_R_rc_20090122.dcm',
        'TransferSyntaxName': 'Explicit VR Little Endian',
        'TransferSyntaxUid': '1.2.840.10008.1.2.1',
        'StudyInstanceUid': '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.1',
        'SopInstanceUid': '1.2.826.0.1.3680043.8.694.211979051425099.5028401604424181',
        'PixelSizeX': 0.094090909,
        'PixelSizeY': 0.094090909,
        'PixelRows': 2294,
        'PixelColumns': 1914,
        'JsonVersion': 1,
        'CryptoVersion': 'H1E1',
        'Hashes': [
          {
            'Key': 'Key1',
            'Value': 'PATIENTID'
          }
        ],
        'Encryptions': []
      },
      'MachineLearning': null,
      'PositioningInputs': null,
      'Scorecard': {
        'FibroglandularTissueVolumeInCm3': 52.7841,
        'BreastVolumeInCm3': 309.6208,
        'VolumetricBreastDensityInPercent': 17.048,
        'ManufacturerDoseInmGy': 2.713,
        'VolparaDoseInmGy': 2.966001,
        'AppliedPressureInkPa': 14.69,
        'AppliedForceInN': 60.0,
        'JsonVersion': 1
      },
      'Calculations': null,
      'Algorithm': {
        'IsInputFileNotDicom': false,
        'IsAlgorithmSuccessful': true,
        'AlgorithmError': null,
        'AlgorithmRunInformation': 'VolparaVersion = 1.5.4.0 | 9561 |',
        'SourceDotOutFileName': null,
        'AlgorithmVersion': '1.5.4.0',
        'AlgorithmBuild': '9561',
        'JsonVersion': 1
      },
      'XlsData': {
        'MammoImageType': '2D',
        'RequestedProcedure': 'Unknown',
        'Folder': 'D:\\local\\Temp\\AlgorithmProcess\\imagecalc_636603168238658972_1',
        'Timestamp': '2018/04/26_05:20:41',
        'OSName': 'Unknown',
        'OSVersion': 'Unknown',
        'CurrentCulture': 'en-US',
        'InstalledUICulture': 'en-US',
        'WriteOutDisplayImage': 'Yes [ From Command Line ]',
        'VolparaVersion': '1.5.4.0 | 9561 |',
        'DICOMTAGManufacturer': 'GE',
        'DICOMTAGDeviceSerialNumber': 'SerialNo-1',
        'DICOMTagDetector_ID': 'DetectorID-1',
        'MaxAllowedKVP': '100 ( current 28 )',
        'BTSF': '1',
        'BTSTF': '0',
        'DetectorType': 'GE [ From Manufacturer ID ]',
        'DoFlatFieldCorrection': 'no [ From Manufacturer ID ]',
        'FSensitivity': '0.011173 [ From File ]',
        'Gain': '0.0100 [ From Manufacturer ID ]',
        'NativePixelSize': '0.1000 [ From Manufacturer ID ]',
        'Offset': '0.0000 [ From Manufacturer ID ]',
        'ScalePixelSize': 'yes [ From Manufacturer ID ]',
        'SourceToDetector': '660.0 [ From Manufacturer ID ]',
        'SupportToDetector': '40.0 [ From Manufacturer ID ]',
        'TubeType': 'GE [ From Manufacturer ID ]',
        'UseNewSlantAlgorithm': 'False [ From Calculations ]',
        'ValidToProcess': 'yes [ From Manufacturer ID ]',
        'BreastSide': 'Right',
        'ChestPosition': 'Right',
        'PectoralPosition': 'Top',
        'MammoView': 'CC',
        'StudyDate': '20090122',
        'OperatorName': 'Technologist-1',
        'PatientDOB': '19600701',
        'PatientAge': '49',
        'PatientID': '0001',
        'DetectorID': 'DetectorID-1',
        'XraySystem': 'GE',
        'TargetPixelSizeMm': '0.355',
        'NearestNeighborResample': null,
        'ResizedPixelSizeMm': '0.3',
        'PectoralSide': 'Top',
        'MedialSide': 'Bottom',
        'PaddleType': 'None',
        'ExposureMas': '139',
        'ExposureTimeMs': '2233',
        'TargetMaterial': 'RHODIUM',
        'FilterMaterial': 'RHODIUM',
        'FilterThicknessMm': '0.025',
        'TubeVoltageKvp': '28',
        'CompressionPlateSlant': '5',
        'HVL_Mm': '0.414365',
        'CompressionForceN': '60',
        'RecordedBreastThicknessMm': '48',
        'InnerBreastStatistics': '( 0; 157; 166; 556; 41587; 87185389.000000; 186742553037.000000; 2096.457764; 308.661255 )',
        'muFatPerMm': '0.045',
        'MethodAllPlaneFit': '0.542165',
        'RejectingMethod1Reason': '-6.65054 > 10 or -6.65054 < -2 or 0.542165 < 0.85',
        'MethodFatPlaneFit': '1',
        'Calculated_Sigma': '5',
        'ComputedSlantAngle': '3.28975',
        'ComputedSlantMm': '11.0017',
        'ComputedBreastThickness': '48',
        'ScatterScaleFactor': '1.00',
        'Scatter': 'Weighted',
        'SegPhaseDE': '35639.82',
        'SegPhaseOD': '0.00',
        'SegPhaseBE': '2511.81',
        'SegPhasePA': '0.00',
        'SegPhaseBA': '5774.67',
        'SegPhaseOA': '0.00',
        'SegPhaseUA': '0.00',
        'SegPhasePD': '0.00',
        'SegSphereDE': '35639.82',
        'SegSphereOD': '0.00',
        'SegSphereBE': '4543.65',
        'SegSpherePA': '0.00',
        'SegSphereBA': '3742.83',
        'SegSphereOA': '0.00',
        'SegSphereUA': '0.00',
        'SegSpherePD': '0.00',
        'ContactAreaMm2': '4084.76',
        'CompressionPressureKPa': '14.69',
        'PFAT_Edge_Zone': '0 0',
        'HintRejectLevel': '48.00 mm',
        'HintIgnoreLevel': '43.20 mm',
        'EntranceDoseInmGy': '11.844',
        'EstimatedEntranceDoseInmGy': '10.0747',
        'Warning': 'No HVL - using estimate',
        'GlandularityPercent': '35.47',
        'VolparaMeanGlandularDoseInmGy': '2.966001',
        'FiftyPercentGlandularDoseInmGy': '2.777193',
        'OrganDose': '0.027130',
        'OrganDoseInmGy': '2.713000',
        'Method2Results309.62152.784117.048': 'APJ2',
        'CorrectionComplete': null,
        'NippleConfidence': '0.996806',
        'NippleConfidenceMessage': '-1|OK||OK|',
        'NippleInProfile': 'Yes',
        'NippleDistanceFromMedialEdgeInMm': '103.299',
        'NippleDistanceFromPosteriorEdgeInMm': '68.6822',
        'NippleCenterDistanceFromMedialEdgeInMm': '103.35',
        'NippleCenterDistanceFromPosteriorEdgeInMm': '68.1',
        'CCPosteriorNippleLineLengthInMm': '68.6822',
        'NippleMedialLateralDistanceInMm': '-5.00139',
        'NippleMedialLateralAngleInDegrees': '-4.16489',
        'BreastCenterToImageCenterDistanceInMm': '-6.45',
        'BreastCenterDistanceFromMedialEdgeInMm': '108.3',
        'BreastEdgeDistanceToPosteriorMedialCornerInMm': '-1',
        'BreastEdgeDistanceToPosteriorLateralCornerInMm': '-1',
        'CleavageDetected': 'No',
        'ShoulderDetected': 'No',
        'MeanDenseThicknessInMm': '12.5249',
        'MaximumDenseThicknessInMm': '24.9225',
        'SDDenseThicknessInMm': '4.91681',
        'MaximumDenseThicknessDistanceFromMedialEdgeInMm': '139.8',
        'MaximumDenseThicknessDistanceFromPosteriorEdgeInMm': '10.5',
        'DensityMapAttenuatingPixelCount': '0',
        'MaximumPercentDensityIn1Cm2Area': '40.6236',
        'MaximumDenseVolumeIn1Cm2AreaInCm3': '1.91113',
        'MaximumDensity1Cm2AreaDistanceFromMedialEdgeInMm': '111',
        'MaximumDensity1Cm2AreaDistanceFromPosteriorEdgeInMm': '25.2',
        'DenseAreaPercent': '92.7117',
        'AreaGreaterThan10mmDenseMm2': '2671.38',
        'HintVolumeCm3': '52.7841',
        'BreastVolumeCm3': '309.6208',
        'VolumetricBreastDensity': '17.0480',
        'Out_BreastVolume': '309.6',
        'Out_FGTV': '52.8',
        'Out_Density': '17.0',
        'Run_Information': 'VolparaVersion = 1.5.4.0 | 9561 |',
        'VolparaOkay': null
      },
      'OtherData': {
        'Projectcompletedsuccessfully': null,
        'PatientID': '0001',
        'FibroglandularTissueVolume': '52.8 cm3',
        'BreastVolume': '309.6 cm3',
        'VolumetricBreastDensity': '17.0 %',
        'RunInformation': 'VolparaVersion = 1.5.4.0 | 9561 |'
      },
      'SpecialData': null,
      'DicomFileMetaInfo': {
        '00020001': {
          'vr': 'OB',
          'InlineBinary': 'AAE='
        },
        '00020002': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.5.1.4.1.1.1.2.1'
          ]
        },
        '00020003': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401604424181'
          ]
        },
        '00020010': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.1.2.1'
          ]
        },
        '00020012': {
          'vr': 'UI',
          'Value': [
            '1.3.6.1.4.1.30071.8'
          ]
        },
        '00020013': {
          'vr': 'SH',
          'Value': [
            'fo-dicom 3.1.0'
          ]
        },
        '00020016': {
          'vr': 'AE',
          'Value': [
            'RD0003FF85A0E1'
          ]
        }
      },
      'DicomHeaderInfo': {
        '00080005': {
          'vr': 'CS',
          'Value': [
            'ISO_IR 100'
          ]
        },
        '00080008': {
          'vr': 'CS',
          'Value': [
            'ORIGINAL',
            'PRIMARY',
            null
          ]
        },
        '00080016': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.5.1.4.1.1.1.2.1'
          ]
        },
        '00080018': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401604424181'
          ]
        },
        '00080020': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080021': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080022': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080023': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080030': {
          'vr': 'TM',
          'Value': [
            '144815'
          ]
        },
        '00080031': {
          'vr': 'TM',
          'Value': [
            '145022'
          ]
        },
        '00080032': {
          'vr': 'TM',
          'Value': [
            '145020'
          ]
        },
        '00080033': {
          'vr': 'TM',
          'Value': [
            '145024'
          ]
        },
        '00080050': {
          'vr': 'SH',
          'Value': [
            '12345'
          ]
        },
        '00080060': {
          'vr': 'CS',
          'Value': [
            'MG'
          ]
        },
        '00080064': {
          'vr': 'CS',
          'Value': [
            'WSD'
          ]
        },
        '00080068': {
          'vr': 'CS',
          'Value': [
            'FOR PROCESSING'
          ]
        },
        '00080070': {
          'vr': 'LO',
          'Value': [
            'GE MEDICAL SYSTEMS'
          ]
        },
        '00080090': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Physician-1'
            }
          ]
        },
        '00081010': {
          'vr': 'SH',
          'Value': [
            'Station-1'
          ]
        },
        '00081030': {
          'vr': 'LO',
          'Value': [
            'B/L MLOS, B/L CCS'
          ]
        },
        '0008103E': {
          'vr': 'LO',
          'Value': [
            'B/L MLOS, B/L CCS'
          ]
        },
        '00081070': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Technologist-1'
            }
          ]
        },
        '00081090': {
          'vr': 'LO',
          'Value': [
            'Senograph DS ADS_43.10.1'
          ]
        },
        '00082218': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  'T-04000'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'SNM3'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'BREAST'
                ]
              }
            }
          ]
        },
        '00090010': {
          'vr': 'LO',
          'Value': [
            'SECTRA_Ident_01'
          ]
        },
        '00091001': {
          'vr': 'SH',
          'Value': [
            '0001223861'
          ]
        },
        '00091002': {
          'vr': 'SH',
          'Value': [
            '01'
          ]
        },
        '00100010': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Anonymous'
            }
          ]
        },
        '00100020': {
          'vr': 'LO',
          'Value': [
            '0001'
          ]
        },
        '00100030': {
          'vr': 'DA',
          'Value': [
            '19600701'
          ]
        },
        '00100040': {
          'vr': 'CS',
          'Value': [
            'F'
          ]
        },
        '00101010': {
          'vr': 'AS',
          'Value': [
            '049Y'
          ]
        },
        '00120062': {
          'vr': 'CS',
          'Value': [
            'YES'
          ]
        },
        '00120063': {
          'vr': 'LO',
          'Value': [
            'Basic Application Confidentiality Profile',
            'Clean Descriptors Option',
            'Retain Longitudinal Temporal Information Modified Dates Option',
            'Retain Patient Characteristics Option',
            'Retain Safe Private Option'
          ]
        },
        '00120064': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113100'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Basic Application Confidentiality Profile'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113105'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Clean Descriptors Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113107'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Longitudinal Temporal Information Modified Dates Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113108'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Patient Characteristics Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113111'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Safe Private Option'
                ]
              }
            }
          ]
        },
        '00180015': {
          'vr': 'CS',
          'Value': [
            'BREAST'
          ]
        },
        '00180060': {
          'vr': 'DS',
          'Value': [
            28
          ]
        },
        '00181000': {
          'vr': 'LO',
          'Value': [
            'SerialNo-1'
          ]
        },
        '00181020': {
          'vr': 'LO',
          'Value': [
            'Ads Application Package VERSION ADS_43.10.1'
          ]
        },
        '00181030': {
          'vr': 'LO',
          'Value': [
            'ROUTINE'
          ]
        },
        '00181110': {
          'vr': 'DS',
          'Value': [
            660
          ]
        },
        '00181111': {
          'vr': 'DS',
          'Value': [
            660
          ]
        },
        '00181114': {
          'vr': 'DS',
          'Value': [
            1
          ]
        },
        '00181147': {
          'vr': 'CS',
          'Value': [
            'RECTANGLE'
          ]
        },
        '00181149': {
          'vr': 'IS',
          'Value': [
            229,
            191
          ]
        },
        '00181150': {
          'vr': 'IS',
          'Value': [
            2233
          ]
        },
        '00181151': {
          'vr': 'IS',
          'Value': [
            61
          ]
        },
        '00181152': {
          'vr': 'IS',
          'Value': [
            139
          ]
        },
        '00181153': {
          'vr': 'IS',
          'Value': [
            138600
          ]
        },
        '00181160': {
          'vr': 'SH',
          'Value': [
            'STRIP'
          ]
        },
        '00181164': {
          'vr': 'DS',
          'Value': [
            0.094090909,
            0.094090909
          ]
        },
        '00181166': {
          'vr': 'CS',
          'Value': [
            'RECIPROCATING',
            'FOCUSED'
          ]
        },
        '00181190': {
          'vr': 'DS',
          'Value': [
            0.3
          ]
        },
        '00181191': {
          'vr': 'CS',
          'Value': [
            'RHODIUM'
          ]
        },
        '001811A0': {
          'vr': 'DS',
          'Value': [
            48
          ]
        },
        '001811A2': {
          'vr': 'DS',
          'Value': [
            60
          ]
        },
        '00181405': {
          'vr': 'IS',
          'Value': [
            11844
          ]
        },
        '00181508': {
          'vr': 'CS',
          'Value': [
            'MAMMOGRAPHIC'
          ]
        },
        '00181510': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00181531': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00181700': {
          'vr': 'CS',
          'Value': [
            'RECTANGULAR'
          ]
        },
        '00181702': {
          'vr': 'IS',
          'Value': [
            0
          ]
        },
        '00181704': {
          'vr': 'IS',
          'Value': [
            1915
          ]
        },
        '00181706': {
          'vr': 'IS',
          'Value': [
            0
          ]
        },
        '00181708': {
          'vr': 'IS',
          'Value': [
            2295
          ]
        },
        '00185101': {
          'vr': 'CS',
          'Value': [
            'CC'
          ]
        },
        '00186000': {
          'vr': 'DS',
          'Value': [
            0.01117304
          ]
        },
        '00187000': {
          'vr': 'CS',
          'Value': [
            'YES'
          ]
        },
        '00187001': {
          'vr': 'DS',
          'Value': [
            30.299999
          ]
        },
        '00187004': {
          'vr': 'CS',
          'Value': [
            'SCINTILLATOR'
          ]
        },
        '00187005': {
          'vr': 'CS',
          'Value': [
            'AREA'
          ]
        },
        '00187006': {
          'vr': 'LT',
          'Value': [
            'DETECTOR VERSION 1.0 MTFCOMP 1.0'
          ]
        },
        '0018700A': {
          'vr': 'SH',
          'Value': [
            'DetectorID-1'
          ]
        },
        '0018700C': {
          'vr': 'DA',
          'Value': [
            '20080717'
          ]
        },
        '0018701A': {
          'vr': 'DS',
          'Value': [
            1,
            1
          ]
        },
        '00187020': {
          'vr': 'DS',
          'Value': [
            0.1,
            0.1
          ]
        },
        '00187022': {
          'vr': 'DS',
          'Value': [
            0.1,
            0.1
          ]
        },
        '00187024': {
          'vr': 'CS',
          'Value': [
            'RECTANGLE'
          ]
        },
        '00187026': {
          'vr': 'DS',
          'Value': [
            192,
            230.4
          ]
        },
        '00187030': {
          'vr': 'DS',
          'Value': [
            5,
            1
          ]
        },
        '00187032': {
          'vr': 'DS',
          'Value': [
            180
          ]
        },
        '00187034': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00187050': {
          'vr': 'CS',
          'Value': [
            'RHODIUM'
          ]
        },
        '00187060': {
          'vr': 'CS',
          'Value': [
            'AUTOMATIC'
          ]
        },
        '00187062': {
          'vr': 'LT',
          'Value': [
            'AOP contrast RECTANGLE 1032 mm 220 mm 180 mm 240 mm EXP DOSE 150236 nGy PRE-EXP DOSE 3412 nGy PRE-EXP THICK 48 mm PRE-EXP COMPO 70 % PRE-EXP KV 27 PRE-EXP TRACK Rh PRE-EXP FILTER Rh PADDLE 0 FLATFIELD no'
          ]
        },
        '00187064': {
          'vr': 'CS',
          'Value': [
            'NORMAL'
          ]
        },
        '0018A001': {
          'vr': 'SQ',
          'Value': [
            {
              '00080070': {
                'vr': 'LO',
                'Value': [
                  'Matakina Technology'
                ]
              },
              '00081090': {
                'vr': 'LO',
                'Value': [
                  'Volpara Data Manager'
                ]
              },
              '00181020': {
                'vr': 'LO',
                'Value': [
                  '1.0.108.0'
                ]
              },
              '0018A002': {
                'vr': 'DT',
                'Value': [
                  '20151208'
                ]
              },
              '0018A003': {
                'vr': 'ST',
                'Value': [
                  'De-identifying Equipment'
                ]
              },
              '0040A170': {
                'vr': 'SQ',
                'Value': [
                  {
                    '00080100': {
                      'vr': 'SH',
                      'Value': [
                        '109104'
                      ]
                    },
                    '00080102': {
                      'vr': 'SH',
                      'Value': [
                        'DCM'
                      ]
                    },
                    '00080104': {
                      'vr': 'LO',
                      'Value': [
                        'De-identifying Equipment'
                      ]
                    }
                  }
                ]
              }
            }
          ]
        },
        '0020000D': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.1'
          ]
        },
        '0020000E': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733'
          ]
        },
        '00200010': {
          'vr': 'SH',
          'Value': [
            'Study-1'
          ]
        },
        '00200011': {
          'vr': 'IS',
          'Value': [
            2
          ]
        },
        '00200013': {
          'vr': 'IS',
          'Value': [
            1
          ]
        },
        '00200020': {
          'vr': 'CS',
          'Value': [
            'P',
            'L'
          ]
        },
        '00200062': {
          'vr': 'CS',
          'Value': [
            'R'
          ]
        },
        '00280002': {
          'vr': 'US',
          'Value': [
            1
          ]
        },
        '00280004': {
          'vr': 'CS',
          'Value': [
            'MONOCHROME1'
          ]
        },
        '00280006': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00280008': {
          'vr': 'IS',
          'Value': [
            1
          ]
        },
        '00280010': {
          'vr': 'US',
          'Value': [
            2294
          ]
        },
        '00280011': {
          'vr': 'US',
          'Value': [
            1914
          ]
        },
        '00280100': {
          'vr': 'US',
          'Value': [
            16
          ]
        },
        '00280101': {
          'vr': 'US',
          'Value': [
            14
          ]
        },
        '00280102': {
          'vr': 'US',
          'Value': [
            13
          ]
        },
        '00280103': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00280300': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00280301': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00280303': {
          'vr': 'CS',
          'Value': [
            'MODIFIED'
          ]
        },
        '00281040': {
          'vr': 'CS',
          'Value': [
            'LIN'
          ]
        },
        '00281041': {
          'vr': 'SS',
          'Value': [
            1
          ]
        },
        '00281052': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00281053': {
          'vr': 'DS',
          'Value': [
            1
          ]
        },
        '00281054': {
          'vr': 'LO',
          'Value': [
            'US'
          ]
        },
        '00281300': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00282110': {
          'vr': 'CS',
          'Value': [
            '00'
          ]
        },
        '00290010': {
          'vr': 'LO',
          'Value': [
            'SECTRA_ImageInfo_01'
          ]
        },
        '00291004': {
          'vr': 'UN',
          'InlineBinary': 'dmlld19zdGF0ZSB7CiAgICBmaTwwPgogICAgc2k8MD4KICAgIGlpPDA+Cn0KZGVmYXVsdF9waXBlCnBpcGVfc3RhdGVzCnBpcGVfb3ZlcmxheXMKY29sbGVjdGlvbjxXOmV3YmN3aXNlOjQ0MTYyMjQgMD4Kb3JkZXI8MTY6Mj4Kc29ydF9vcmRlcjxhc2NlbmRpbmc+CnNvcnRfb3BlcmF0aW9uPG5vbmU+CgAK'
        },
        '00400275': {
          'vr': 'SQ',
          'Value': [
            {
              '00400007': {
                'vr': 'LO',
                'Value': [
                  'B/L MLOS, B/L CCS'
                ]
              }
            }
          ]
        },
        '00400302': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00400306': {
          'vr': 'DS',
          'Value': [
            612
          ]
        },
        '00400310': {
          'vr': 'ST',
          'Value': [
            '69 %'
          ]
        },
        '00400316': {
          'vr': 'DS',
          'Value': [
            0.02713
          ]
        },
        '00400318': {
          'vr': 'CS',
          'Value': [
            'BREAST'
          ]
        },
        '00400555': {
          'vr': 'SQ'
        },
        '00408302': {
          'vr': 'DS',
          'Value': [
            11.844
          ]
        },
        '00450010': {
          'vr': 'LO',
          'Value': [
            'GEMS_SENO_02'
          ]
        },
        '00451006': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '0045101B': {
          'vr': 'CS',
          'Value': [
            'RCC'
          ]
        },
        '00451020': {
          'vr': 'DS',
          'Value': [
            2076.0298
          ]
        },
        '00451026': {
          'vr': 'OB',
          'InlineBinary': 'ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgAA=='
        },
        '00451029': {
          'vr': 'DS',
          'Value': [
            1806,
            5.5
          ]
        },
        '0045102A': {
          'vr': 'IS',
          'Value': [
            -1
          ]
        },
        '0045102B': {
          'vr': 'IS',
          'Value': [
            -1
          ]
        },
        '00451049': {
          'vr': 'UN',
          'InlineBinary': 'NDg='
        },
        '00451050': {
          'vr': 'UN',
          'InlineBinary': 'MS4yLjg0MC4xMTM2MTkuMi42Ni4yMjAzODE2MTg4LjE0OTkwOTAxMzAxNDUwMjQuNjQyAA=='
        },
        '00451051': {
          'vr': 'UN',
          'InlineBinary': 'MS4yLjg0MC4xMTM2MTkuMi42Ni4yMjAzODE2MTg4LjIzMjUwMDkwMTMwMTQ1MDIyLjEwMDA0'
        },
        '00451071': {
          'vr': 'UN',
          'InlineBinary': 'ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgAA=='
        },
        '00451072': {
          'vr': 'UN',
          'InlineBinary': 'NVw1IA=='
        },
        '00540220': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  'R-10242'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'SNM3'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'cranio-caudal'
                ]
              },
              '00540222': {
                'vr': 'SQ'
              }
            }
          ]
        },
        '20500020': {
          'vr': 'CS',
          'Value': [
            'INVERSE'
          ]
        }
      },
      'ImageResultCreationTimeUtc': '2018-04-26T05:22:05.5533094Z'
    },
    {
      'JsonVersion': 1,
      'SourceImage': {
        'DicomImageFilePath': '0001_GE_R_lc_20090122.dcm',
        'TransferSyntaxName': 'Explicit VR Little Endian',
        'TransferSyntaxUid': '1.2.840.10008.1.2.1',
        'StudyInstanceUid': '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.1',
        'SopInstanceUid': '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.0',
        'PixelSizeX': 0.094090909,
        'PixelSizeY': 0.094090909,
        'PixelRows': 2294,
        'PixelColumns': 1914,
        'JsonVersion': 1,
        'CryptoVersion': 'H1E1',
        'Hashes': [
          {
            'Key': 'Key1',
            'Value': 'PATIENTID'
          }
        ],
        'Encryptions': []
      },
      'MachineLearning': null,
      'PositioningInputs': null,
      'Scorecard': {
        'FibroglandularTissueVolumeInCm3': 47.2902,
        'BreastVolumeInCm3': 373.6459,
        'VolumetricBreastDensityInPercent': 12.6564,
        'ManufacturerDoseInmGy': 2.949,
        'VolparaDoseInmGy': 3.248312,
        'AppliedPressureInkPa': 11.11,
        'AppliedForceInN': 50.0,
        'JsonVersion': 1
      },
      'Calculations': null,
      'Algorithm': {
        'IsInputFileNotDicom': false,
        'IsAlgorithmSuccessful': true,
        'AlgorithmError': null,
        'AlgorithmRunInformation': 'VolparaVersion = 1.5.4.0 | 9561 |',
        'SourceDotOutFileName': null,
        'AlgorithmVersion': '1.5.4.0',
        'AlgorithmBuild': '9561',
        'JsonVersion': 1
      },
      'XlsData': {
        'MammoImageType': '2D',
        'RequestedProcedure': 'Unknown',
        'Folder': 'D:\\local\\Temp\\AlgorithmProcess\\imagecalc_636603168238658972_4',
        'Timestamp': '2018/04/26_05:20:41',
        'OSName': 'Unknown',
        'OSVersion': 'Unknown',
        'CurrentCulture': 'en-US',
        'InstalledUICulture': 'en-US',
        'WriteOutDisplayImage': 'Yes [ From Command Line ]',
        'VolparaVersion': '1.5.4.0 | 9561 |',
        'DICOMTAGManufacturer': 'GE',
        'DICOMTAGDeviceSerialNumber': 'SerialNo-1',
        'DICOMTagDetector_ID': 'DetectorID-1',
        'MaxAllowedKVP': '100 ( current 28 )',
        'BTSF': '1',
        'BTSTF': '0',
        'DetectorType': 'GE [ From Manufacturer ID ]',
        'DoFlatFieldCorrection': 'no [ From Manufacturer ID ]',
        'FSensitivity': '0.011173 [ From File ]',
        'Gain': '0.0100 [ From Manufacturer ID ]',
        'NativePixelSize': '0.1000 [ From Manufacturer ID ]',
        'Offset': '0.0000 [ From Manufacturer ID ]',
        'ScalePixelSize': 'yes [ From Manufacturer ID ]',
        'SourceToDetector': '660.0 [ From Manufacturer ID ]',
        'SupportToDetector': '40.0 [ From Manufacturer ID ]',
        'TubeType': 'GE [ From Manufacturer ID ]',
        'UseNewSlantAlgorithm': 'False [ From Calculations ]',
        'ValidToProcess': 'yes [ From Manufacturer ID ]',
        'BreastSide': 'Left',
        'ChestPosition': 'Left',
        'PectoralPosition': 'Top',
        'MammoView': 'CC',
        'StudyDate': '20090122',
        'OperatorName': 'Technologist-1',
        'PatientDOB': '19600701',
        'PatientAge': '49',
        'PatientID': '0001',
        'DetectorID': 'DetectorID-1',
        'XraySystem': 'GE',
        'TargetPixelSizeMm': '0.355',
        'NearestNeighborResample': null,
        'ResizedPixelSizeMm': '0.3',
        'PectoralSide': 'Top',
        'MedialSide': 'Bottom',
        'PaddleType': 'None',
        'ExposureMas': '155',
        'ExposureTimeMs': '2495',
        'TargetMaterial': 'RHODIUM',
        'FilterMaterial': 'RHODIUM',
        'FilterThicknessMm': '0.025',
        'TubeVoltageKvp': '28',
        'CompressionPlateSlant': '5',
        'HVL_Mm': '0.414365',
        'CompressionForceN': '50',
        'RecordedBreastThicknessMm': '51',
        'InnerBreastStatistics': '( 0; 168; 164; 575; 46903; 102853078.000000; 232603699036.000000; 2192.889160; 387.926880 )',
        'muFatPerMm': '0.045',
        'MethodAllPlaneFit': '0.362066',
        'RejectingMethod1Reason': '-6.60858 > 10 or -6.60858 < -2 or 0.362066 < 0.85',
        'MethodFatPlaneFit': '0.997815',
        'Calculated_Sigma': '5',
        'ComputedSlantAngle': '2.07312',
        'ComputedSlantMm': '6.9284',
        'ComputedBreastThickness': '51',
        'ScatterScaleFactor': '1.00',
        'Scatter': 'Weighted',
        'SegPhaseDE': '34506.36',
        'SegPhaseOD': '0.00',
        'SegPhaseBE': '2694.51',
        'SegPhasePA': '0.00',
        'SegPhaseBA': '6725.43',
        'SegPhaseOA': '0.00',
        'SegPhaseUA': '0.00',
        'SegPhasePD': '0.00',
        'SegSphereDE': '34506.36',
        'SegSphereOD': '0.00',
        'SegSphereBE': '5198.67',
        'SegSpherePA': '0.00',
        'SegSphereBA': '4221.27',
        'SegSphereOA': '0.00',
        'SegSphereUA': '0.00',
        'SegSpherePD': '0.00',
        'ContactAreaMm2': '4500.82',
        'CompressionPressureKPa': '11.11',
        'PFAT_Edge_Zone': '0 0',
        'HintRejectLevel': '51.00 mm',
        'HintIgnoreLevel': '45.90 mm',
        'EntranceDoseInmGy': '13.358',
        'EstimatedEntranceDoseInmGy': '11.3512',
        'Warning': 'No HVL - using estimate',
        'GlandularityPercent': '27.76',
        'VolparaMeanGlandularDoseInmGy': '3.248312',
        'FiftyPercentGlandularDoseInmGy': '2.933777',
        'OrganDose': '0.029490',
        'OrganDoseInmGy': '2.949000',
        'Method2Results373.64647.290212.6564': null,
        'CorrectionComplete': null,
        'NippleConfidence': '0.973113',
        'NippleConfidenceMessage': '-1|OK||OK|',
        'NippleInProfile': 'No',
        'NippleDistanceFromMedialEdgeInMm': '107.842',
        'NippleDistanceFromPosteriorEdgeInMm': '73.7483',
        'NippleCenterDistanceFromMedialEdgeInMm': '108.199',
        'NippleCenterDistanceFromPosteriorEdgeInMm': '71.0521',
        'CCPosteriorNippleLineLengthInMm': '73.7483',
        'NippleMedialLateralDistanceInMm': '-2.85767',
        'NippleMedialLateralAngleInDegrees': '-2.21904',
        'BreastCenterToImageCenterDistanceInMm': '-4.05',
        'BreastCenterDistanceFromMedialEdgeInMm': '110.7',
        'BreastEdgeDistanceToPosteriorMedialCornerInMm': '-1',
        'BreastEdgeDistanceToPosteriorLateralCornerInMm': '-1',
        'CleavageDetected': 'No',
        'ShoulderDetected': 'No',
        'MeanDenseThicknessInMm': '10.5346',
        'MaximumDenseThicknessInMm': '30.4352',
        'SDDenseThicknessInMm': '5.71302',
        'MaximumDenseThicknessDistanceFromMedialEdgeInMm': '127.2',
        'MaximumDenseThicknessDistanceFromPosteriorEdgeInMm': '25.2',
        'DensityMapAttenuatingPixelCount': '0',
        'MaximumPercentDensityIn1Cm2Area': '37.6632',
        'MaximumDenseVolumeIn1Cm2AreaInCm3': '1.8826',
        'MaximumDensity1Cm2AreaDistanceFromMedialEdgeInMm': '117',
        'MaximumDensity1Cm2AreaDistanceFromPosteriorEdgeInMm': '30',
        'DenseAreaPercent': '80.5556',
        'AreaGreaterThan10mmDenseMm2': '2344.32',
        'HintVolumeCm3': '47.2902',
        'BreastVolumeCm3': '373.6459',
        'VolumetricBreastDensity': '12.6564',
        'Out_BreastVolume': '373.6',
        'Out_FGTV': '47.3',
        'Out_Density': '12.7',
        'Run_Information': 'VolparaVersion = 1.5.4.0 | 9561 |',
        'VolparaOkay': null
      },
      'OtherData': {
        'Projectcompletedsuccessfully': null,
        'PatientID': '0001',
        'FibroglandularTissueVolume': '47.3 cm3',
        'BreastVolume': '373.6 cm3',
        'VolumetricBreastDensity': '12.7 %',
        'RunInformation': 'VolparaVersion = 1.5.4.0 | 9561 |'
      },
      'SpecialData': null,
      'DicomFileMetaInfo': {
        '00020001': {
          'vr': 'OB',
          'InlineBinary': 'AAE='
        },
        '00020002': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.5.1.4.1.1.1.2.1'
          ]
        },
        '00020003': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.0'
          ]
        },
        '00020010': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.1.2.1'
          ]
        },
        '00020012': {
          'vr': 'UI',
          'Value': [
            '1.3.6.1.4.1.30071.8'
          ]
        },
        '00020013': {
          'vr': 'SH',
          'Value': [
            'fo-dicom 3.1.0'
          ]
        },
        '00020016': {
          'vr': 'AE',
          'Value': [
            'RD0003FF85A0E1'
          ]
        }
      },
      'DicomHeaderInfo': {
        '00080005': {
          'vr': 'CS',
          'Value': [
            'ISO_IR 100'
          ]
        },
        '00080008': {
          'vr': 'CS',
          'Value': [
            'ORIGINAL',
            'PRIMARY',
            null
          ]
        },
        '00080016': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.5.1.4.1.1.1.2.1'
          ]
        },
        '00080018': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.0'
          ]
        },
        '00080020': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080021': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080022': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080023': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080030': {
          'vr': 'TM',
          'Value': [
            '144815'
          ]
        },
        '00080031': {
          'vr': 'TM',
          'Value': [
            '145027'
          ]
        },
        '00080032': {
          'vr': 'TM',
          'Value': [
            '145059'
          ]
        },
        '00080033': {
          'vr': 'TM',
          'Value': [
            '145104'
          ]
        },
        '00080050': {
          'vr': 'SH',
          'Value': [
            '12345'
          ]
        },
        '00080060': {
          'vr': 'CS',
          'Value': [
            'MG'
          ]
        },
        '00080064': {
          'vr': 'CS',
          'Value': [
            'WSD'
          ]
        },
        '00080068': {
          'vr': 'CS',
          'Value': [
            'FOR PROCESSING'
          ]
        },
        '00080070': {
          'vr': 'LO',
          'Value': [
            'GE MEDICAL SYSTEMS'
          ]
        },
        '00080090': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Physician-1'
            }
          ]
        },
        '00081010': {
          'vr': 'SH',
          'Value': [
            'Station-1'
          ]
        },
        '00081030': {
          'vr': 'LO',
          'Value': [
            'B/L MLOS, B/L CCS'
          ]
        },
        '0008103E': {
          'vr': 'LO',
          'Value': [
            'B/L MLOS, B/L CCS'
          ]
        },
        '00081070': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Technologist-1'
            }
          ]
        },
        '00081090': {
          'vr': 'LO',
          'Value': [
            'Senograph DS ADS_43.10.1'
          ]
        },
        '00082218': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  'T-04000'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'SNM3'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'BREAST'
                ]
              }
            }
          ]
        },
        '00090010': {
          'vr': 'LO',
          'Value': [
            'SECTRA_Ident_01'
          ]
        },
        '00091001': {
          'vr': 'SH',
          'Value': [
            '0001223861'
          ]
        },
        '00091002': {
          'vr': 'SH',
          'Value': [
            '01'
          ]
        },
        '00100010': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Anonymous'
            }
          ]
        },
        '00100020': {
          'vr': 'LO',
          'Value': [
            '0001'
          ]
        },
        '00100030': {
          'vr': 'DA',
          'Value': [
            '19600701'
          ]
        },
        '00100040': {
          'vr': 'CS',
          'Value': [
            'F'
          ]
        },
        '00101010': {
          'vr': 'AS',
          'Value': [
            '049Y'
          ]
        },
        '00120062': {
          'vr': 'CS',
          'Value': [
            'YES'
          ]
        },
        '00120063': {
          'vr': 'LO',
          'Value': [
            'Basic Application Confidentiality Profile',
            'Clean Descriptors Option',
            'Retain Longitudinal Temporal Information Modified Dates Option',
            'Retain Patient Characteristics Option',
            'Retain Safe Private Option'
          ]
        },
        '00120064': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113100'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Basic Application Confidentiality Profile'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113105'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Clean Descriptors Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113107'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Longitudinal Temporal Information Modified Dates Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113108'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Patient Characteristics Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113111'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Safe Private Option'
                ]
              }
            }
          ]
        },
        '00180015': {
          'vr': 'CS',
          'Value': [
            'BREAST'
          ]
        },
        '00180060': {
          'vr': 'DS',
          'Value': [
            28
          ]
        },
        '00181000': {
          'vr': 'LO',
          'Value': [
            'SerialNo-1'
          ]
        },
        '00181020': {
          'vr': 'LO',
          'Value': [
            'Ads Application Package VERSION ADS_43.10.1'
          ]
        },
        '00181030': {
          'vr': 'LO',
          'Value': [
            'ROUTINE'
          ]
        },
        '00181110': {
          'vr': 'DS',
          'Value': [
            660
          ]
        },
        '00181111': {
          'vr': 'DS',
          'Value': [
            660
          ]
        },
        '00181114': {
          'vr': 'DS',
          'Value': [
            1
          ]
        },
        '00181147': {
          'vr': 'CS',
          'Value': [
            'RECTANGLE'
          ]
        },
        '00181149': {
          'vr': 'IS',
          'Value': [
            229,
            191
          ]
        },
        '00181150': {
          'vr': 'IS',
          'Value': [
            2495
          ]
        },
        '00181151': {
          'vr': 'IS',
          'Value': [
            62
          ]
        },
        '00181152': {
          'vr': 'IS',
          'Value': [
            155
          ]
        },
        '00181153': {
          'vr': 'IS',
          'Value': [
            154800
          ]
        },
        '00181160': {
          'vr': 'SH',
          'Value': [
            'STRIP'
          ]
        },
        '00181164': {
          'vr': 'DS',
          'Value': [
            0.094090909,
            0.094090909
          ]
        },
        '00181166': {
          'vr': 'CS',
          'Value': [
            'RECIPROCATING',
            'FOCUSED'
          ]
        },
        '00181190': {
          'vr': 'DS',
          'Value': [
            0.3
          ]
        },
        '00181191': {
          'vr': 'CS',
          'Value': [
            'RHODIUM'
          ]
        },
        '001811A0': {
          'vr': 'DS',
          'Value': [
            51
          ]
        },
        '001811A2': {
          'vr': 'DS',
          'Value': [
            50
          ]
        },
        '00181405': {
          'vr': 'IS',
          'Value': [
            13358
          ]
        },
        '00181508': {
          'vr': 'CS',
          'Value': [
            'MAMMOGRAPHIC'
          ]
        },
        '00181510': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00181531': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00181700': {
          'vr': 'CS',
          'Value': [
            'RECTANGULAR'
          ]
        },
        '00181702': {
          'vr': 'IS',
          'Value': [
            0
          ]
        },
        '00181704': {
          'vr': 'IS',
          'Value': [
            1915
          ]
        },
        '00181706': {
          'vr': 'IS',
          'Value': [
            0
          ]
        },
        '00181708': {
          'vr': 'IS',
          'Value': [
            2295
          ]
        },
        '00185101': {
          'vr': 'CS',
          'Value': [
            'CC'
          ]
        },
        '00186000': {
          'vr': 'DS',
          'Value': [
            0.01117304
          ]
        },
        '00187000': {
          'vr': 'CS',
          'Value': [
            'YES'
          ]
        },
        '00187001': {
          'vr': 'DS',
          'Value': [
            30.299999
          ]
        },
        '00187004': {
          'vr': 'CS',
          'Value': [
            'SCINTILLATOR'
          ]
        },
        '00187005': {
          'vr': 'CS',
          'Value': [
            'AREA'
          ]
        },
        '00187006': {
          'vr': 'LT',
          'Value': [
            'DETECTOR VERSION 1.0 MTFCOMP 1.0'
          ]
        },
        '0018700A': {
          'vr': 'SH',
          'Value': [
            'DetectorID-1'
          ]
        },
        '0018700C': {
          'vr': 'DA',
          'Value': [
            '20080717'
          ]
        },
        '0018701A': {
          'vr': 'DS',
          'Value': [
            1,
            1
          ]
        },
        '00187020': {
          'vr': 'DS',
          'Value': [
            0.1,
            0.1
          ]
        },
        '00187022': {
          'vr': 'DS',
          'Value': [
            0.1,
            0.1
          ]
        },
        '00187024': {
          'vr': 'CS',
          'Value': [
            'RECTANGLE'
          ]
        },
        '00187026': {
          'vr': 'DS',
          'Value': [
            192,
            230.4
          ]
        },
        '00187030': {
          'vr': 'DS',
          'Value': [
            5,
            1
          ]
        },
        '00187032': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00187034': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00187050': {
          'vr': 'CS',
          'Value': [
            'RHODIUM'
          ]
        },
        '00187060': {
          'vr': 'CS',
          'Value': [
            'AUTOMATIC'
          ]
        },
        '00187062': {
          'vr': 'LT',
          'Value': [
            'AOP contrast RECTANGLE 1062 mm 280 mm 180 mm 240 mm EXP DOSE 150327 nGy PRE-EXP DOSE 3835 nGy PRE-EXP THICK 50 mm PRE-EXP COMPO 65 % PRE-EXP KV 28 PRE-EXP TRACK Rh PRE-EXP FILTER Rh PADDLE 0 FLATFIELD no'
          ]
        },
        '00187064': {
          'vr': 'CS',
          'Value': [
            'NORMAL'
          ]
        },
        '0018A001': {
          'vr': 'SQ',
          'Value': [
            {
              '00080070': {
                'vr': 'LO',
                'Value': [
                  'Matakina Technology'
                ]
              },
              '00081090': {
                'vr': 'LO',
                'Value': [
                  'Volpara Data Manager'
                ]
              },
              '00181020': {
                'vr': 'LO',
                'Value': [
                  '1.0.108.0'
                ]
              },
              '0018A002': {
                'vr': 'DT',
                'Value': [
                  '20151208'
                ]
              },
              '0018A003': {
                'vr': 'ST',
                'Value': [
                  'De-identifying Equipment'
                ]
              },
              '0040A170': {
                'vr': 'SQ',
                'Value': [
                  {
                    '00080100': {
                      'vr': 'SH',
                      'Value': [
                        '109104'
                      ]
                    },
                    '00080102': {
                      'vr': 'SH',
                      'Value': [
                        'DCM'
                      ]
                    },
                    '00080104': {
                      'vr': 'LO',
                      'Value': [
                        'De-identifying Equipment'
                      ]
                    }
                  }
                ]
              }
            }
          ]
        },
        '0020000D': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.1'
          ]
        },
        '0020000E': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733'
          ]
        },
        '00200010': {
          'vr': 'SH',
          'Value': [
            'Study-1'
          ]
        },
        '00200011': {
          'vr': 'IS',
          'Value': [
            1
          ]
        },
        '00200013': {
          'vr': 'IS',
          'Value': [
            2
          ]
        },
        '00200020': {
          'vr': 'CS',
          'Value': [
            'A',
            'R'
          ]
        },
        '00200062': {
          'vr': 'CS',
          'Value': [
            'L'
          ]
        },
        '00280002': {
          'vr': 'US',
          'Value': [
            1
          ]
        },
        '00280004': {
          'vr': 'CS',
          'Value': [
            'MONOCHROME1'
          ]
        },
        '00280006': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00280008': {
          'vr': 'IS',
          'Value': [
            1
          ]
        },
        '00280010': {
          'vr': 'US',
          'Value': [
            2294
          ]
        },
        '00280011': {
          'vr': 'US',
          'Value': [
            1914
          ]
        },
        '00280100': {
          'vr': 'US',
          'Value': [
            16
          ]
        },
        '00280101': {
          'vr': 'US',
          'Value': [
            14
          ]
        },
        '00280102': {
          'vr': 'US',
          'Value': [
            13
          ]
        },
        '00280103': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00280300': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00280301': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00280303': {
          'vr': 'CS',
          'Value': [
            'MODIFIED'
          ]
        },
        '00281040': {
          'vr': 'CS',
          'Value': [
            'LIN'
          ]
        },
        '00281041': {
          'vr': 'SS',
          'Value': [
            1
          ]
        },
        '00281052': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00281053': {
          'vr': 'DS',
          'Value': [
            1
          ]
        },
        '00281054': {
          'vr': 'LO',
          'Value': [
            'US'
          ]
        },
        '00281300': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00282110': {
          'vr': 'CS',
          'Value': [
            '00'
          ]
        },
        '00290010': {
          'vr': 'LO',
          'Value': [
            'SECTRA_ImageInfo_01'
          ]
        },
        '00291004': {
          'vr': 'UN',
          'InlineBinary': 'dmlld19zdGF0ZSB7CiAgICBmaTwwPgogICAgc2k8MD4KICAgIGlpPDA+Cn0KZGVmYXVsdF9waXBlCnBpcGVfc3RhdGVzCnBpcGVfb3ZlcmxheXMKY29sbGVjdGlvbjxXOmV3YmN3aXNlOjQ0MTYyMjIgMD4Kb3JkZXI8MTY6MT4Kc29ydF9vcmRlcjxhc2NlbmRpbmc+CnNvcnRfb3BlcmF0aW9uPG5vbmU+CgAK'
        },
        '00400275': {
          'vr': 'SQ',
          'Value': [
            {
              '00400007': {
                'vr': 'LO',
                'Value': [
                  'B/L MLOS, B/L CCS'
                ]
              }
            }
          ]
        },
        '00400302': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00400306': {
          'vr': 'DS',
          'Value': [
            609
          ]
        },
        '00400310': {
          'vr': 'ST',
          'Value': [
            '63 %'
          ]
        },
        '00400316': {
          'vr': 'DS',
          'Value': [
            0.02949
          ]
        },
        '00400318': {
          'vr': 'CS',
          'Value': [
            'BREAST'
          ]
        },
        '00400555': {
          'vr': 'SQ'
        },
        '00408302': {
          'vr': 'DS',
          'Value': [
            13.358
          ]
        },
        '00450010': {
          'vr': 'LO',
          'Value': [
            'GEMS_SENO_02'
          ]
        },
        '00451006': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '0045101B': {
          'vr': 'CS',
          'Value': [
            'LCC'
          ]
        },
        '00451020': {
          'vr': 'DS',
          'Value': [
            2020.0503
          ]
        },
        '00451026': {
          'vr': 'OB',
          'InlineBinary': 'ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgAA=='
        },
        '00451029': {
          'vr': 'DS',
          'Value': [
            1757,
            5.5
          ]
        },
        '0045102A': {
          'vr': 'IS',
          'Value': [
            -1
          ]
        },
        '0045102B': {
          'vr': 'IS',
          'Value': [
            -1
          ]
        },
        '00451049': {
          'vr': 'UN',
          'InlineBinary': 'NTA='
        },
        '00451050': {
          'vr': 'UN',
          'InlineBinary': 'MS4yLjg0MC4xMTM2MTkuMi42Ni4yMjAzODE2MTg4LjE0OTkwOTAxMzAxNDUxMDQuNjQ2AA=='
        },
        '00451051': {
          'vr': 'UN',
          'InlineBinary': 'MS4yLjg0MC4xMTM2MTkuMi42Ni4yMjAzODE2MTg4LjIzMjUwMDkwMTMwMTQ1MDI3LjEwMDA2'
        },
        '00451071': {
          'vr': 'UN',
          'InlineBinary': 'ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgAA=='
        },
        '00451072': {
          'vr': 'UN',
          'InlineBinary': 'NVwxIA=='
        },
        '00540220': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  'R-10242'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'SNM3'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'cranio-caudal'
                ]
              },
              '00540222': {
                'vr': 'SQ'
              }
            }
          ]
        },
        '20500020': {
          'vr': 'CS',
          'Value': [
            'INVERSE'
          ]
        }
      },
      'ImageResultCreationTimeUtc': '2018-04-26T05:22:06.4293376Z'
    },
    {
      'JsonVersion': 1,
      'SourceImage': {
        'DicomImageFilePath': '0001_GE_R_lm_20090122.dcm',
        'TransferSyntaxName': 'Explicit VR Little Endian',
        'TransferSyntaxUid': '1.2.840.10008.1.2.1',
        'StudyInstanceUid': '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.1',
        'SopInstanceUid': '1.2.826.0.1.3680043.8.694.211979051425099.5028401561177965',
        'PixelSizeX': 0.094090909,
        'PixelSizeY': 0.094090909,
        'PixelRows': 2294,
        'PixelColumns': 1914,
        'JsonVersion': 1,
        'CryptoVersion': 'H1E1',
        'Hashes': [
          {
            'Key': 'Key1',
            'Value': 'PATIENTID'
          }
        ],
        'Encryptions': []
      },
      'MachineLearning': null,
      'PositioningInputs': null,
      'Scorecard': {
        'FibroglandularTissueVolumeInCm3': 51.4824,
        'BreastVolumeInCm3': 384.5103,
        'VolumetricBreastDensityInPercent': 13.3891,
        'ManufacturerDoseInmGy': 3.076,
        'VolparaDoseInmGy': 3.36867,
        'AppliedPressureInkPa': 13.31,
        'AppliedForceInN': 110.0,
        'JsonVersion': 1
      },
      'Calculations': null,
      'Algorithm': {
        'IsInputFileNotDicom': false,
        'IsAlgorithmSuccessful': true,
        'AlgorithmError': null,
        'AlgorithmRunInformation': 'VolparaVersion = 1.5.4.0 | 9561 |',
        'SourceDotOutFileName': null,
        'AlgorithmVersion': '1.5.4.0',
        'AlgorithmBuild': '9561',
        'JsonVersion': 1
      },
      'XlsData': {
        'MammoImageType': '2D',
        'RequestedProcedure': 'Unknown',
        'Folder': 'D:\\local\\Temp\\AlgorithmProcess\\imagecalc_636603168238658972_3',
        'Timestamp': '2018/04/26_05:20:41',
        'OSName': 'Unknown',
        'OSVersion': 'Unknown',
        'CurrentCulture': 'en-US',
        'InstalledUICulture': 'en-US',
        'WriteOutDisplayImage': 'Yes [ From Command Line ]',
        'VolparaVersion': '1.5.4.0 | 9561 |',
        'DICOMTAGManufacturer': 'GE',
        'DICOMTAGDeviceSerialNumber': 'SerialNo-1',
        'DICOMTagDetector_ID': 'DetectorID-1',
        'MaxAllowedKVP': '100 ( current 28 )',
        'BTSF': '1',
        'BTSTF': '0',
        'DetectorType': 'GE [ From Manufacturer ID ]',
        'DoFlatFieldCorrection': 'no [ From Manufacturer ID ]',
        'FSensitivity': '0.011173 [ From File ]',
        'Gain': '0.0100 [ From Manufacturer ID ]',
        'NativePixelSize': '0.1000 [ From Manufacturer ID ]',
        'Offset': '0.0000 [ From Manufacturer ID ]',
        'ScalePixelSize': 'yes [ From Manufacturer ID ]',
        'SourceToDetector': '660.0 [ From Manufacturer ID ]',
        'SupportToDetector': '40.0 [ From Manufacturer ID ]',
        'TubeType': 'GE [ From Manufacturer ID ]',
        'UseNewSlantAlgorithm': 'False [ From Calculations ]',
        'ValidToProcess': 'yes [ From Manufacturer ID ]',
        'BreastSide': 'Left',
        'ChestPosition': 'Left',
        'PectoralPosition': 'Top',
        'MammoView': 'MLO',
        'StudyDate': '20090122',
        'OperatorName': 'Technologist-1',
        'PatientDOB': '19600701',
        'PatientAge': '49',
        'PatientID': '0001',
        'DetectorID': 'DetectorID-1',
        'XraySystem': 'GE',
        'TargetPixelSizeMm': '0.355',
        'NearestNeighborResample': null,
        'ResizedPixelSizeMm': '0.3',
        'PectoralSide': 'Top',
        'PaddleType': 'None',
        'ExposureMas': '166',
        'ExposureTimeMs': '2684',
        'TargetMaterial': 'RHODIUM',
        'FilterMaterial': 'RHODIUM',
        'FilterThicknessMm': '0.025',
        'TubeVoltageKvp': '28',
        'CompressionPlateSlant': '5',
        'HVL_Mm': '0.414365',
        'CompressionForceN': '110',
        'RecordedBreastThicknessMm': '52',
        'PectoralAngleDegrees': '18.000',
        'PectoralAngleConfidence': '1.000',
        'InnerBreastStatistics': '( 6; 206; 34; 553; 41201; 93219625.000000; 218697102973.000000; 2262.557373; 434.611969 )',
        'muFatPerMm': '0.045',
        'MethodAllPlaneFit': '0.393551',
        'RejectingMethod1Reason': '-2.25925 > 10 or -2.25925 < -2 or 0.393551 < 0.85',
        'MethodFatPlaneFit': '0.981958',
        'Calculated_Sigma': '5',
        'ComputedSlantAngle': '3.09252',
        'ComputedSlantMm': '10.3408',
        'ComputedBreastThickness': '52',
        'ScatterScaleFactor': '1.00',
        'Scatter': 'Weighted',
        'SegPhaseDE': '29428.38',
        'SegPhaseOD': '0.00',
        'SegPhaseBE': '3995.10',
        'SegPhasePA': '4839.21',
        'SegPhaseBA': '5663.61',
        'SegPhaseOA': '0.00',
        'SegPhaseUA': '0.00',
        'SegPhasePD': '0.00',
        'SegSphereDE': '29428.38',
        'SegSphereOD': '0.00',
        'SegSphereBE': '5950.62',
        'SegSpherePA': '4839.21',
        'SegSphereBA': '3708.09',
        'SegSphereOA': '0.00',
        'SegSphereUA': '0.00',
        'SegSpherePD': '0.00',
        'ContactAreaMm2': '8262.73',
        'CompressionPressureKPa': '13.31',
        'PFAT_Edge_Zone': '0 0',
        'HintRejectLevel': '52.00 mm',
        'HintIgnoreLevel': '46.80 mm',
        'EntranceDoseInmGy': '14.388',
        'EstimatedEntranceDoseInmGy': '12.1988',
        'Warning': 'No HVL - using estimate',
        'GlandularityPercent': '32.02',
        'VolparaMeanGlandularDoseInmGy': '3.368670',
        'FiftyPercentGlandularDoseInmGy': '3.098931',
        'OrganDose': '0.030760',
        'OrganDoseInmGy': '3.076000',
        'Method2Results384.5151.482413.3891': null,
        'CorrectionComplete': null,
        'NippleConfidence': '0.992777',
        'NippleConfidenceMessage': '|OK||OK||OK|',
        'NippleInProfile': 'No',
        'NippleDistanceFromSuperiorEdgeInMm': '136.946',
        'NippleDistanceFromPosteriorEdgeInMm': '84.5768',
        'NippleCenterDistanceFromSuperiorEdgeInMm': '135.524',
        'NippleCenterDistanceFromPosteriorEdgeInMm': '79.4874',
        'MLOPosteriorNippleLineLengthInMm': '70.0348',
        'NippleLineLengthInMm': '84.5768',
        'PNLToInferiorPectoralMuscleVerticalLengthInMm': '-56.0645',
        'PectoralSkinFoldPresent': 'No',
        'NippleToInferiorPectoralMuscleVerticalLengthInMm': '-34.6536',
        'SuperiorPectoralWidthInMm': '56.4',
        'PosteriorPectoralLengthInMm': '171.6',
        'PectoralShape': 'CONCAVE',
        'ImfMaxDistanceMm': '7.1115',
        'InframammaryFoldVisible': 'Yes',
        'InframammaryFoldAreaInMm2': '197.91',
        'ImfAngleInDegrees': '142.853',
        'ImfSkinFoldPresent': 'No',
        'MeanDenseThicknessInMm': '12.6096',
        'MaximumDenseThicknessInMm': '32.7111',
        'SDDenseThicknessInMm': '5.65988',
        'MaximumDenseThicknessDistanceFromSuperiorEdgeInMm': '117',
        'MaximumDenseThicknessDistanceFromPosteriorEdgeInMm': '43.8',
        'DensityMapAttenuatingPixelCount': '0',
        'MaximumPercentDensityIn1Cm2Area': '43.0958',
        'MaximumDenseVolumeIn1Cm2AreaInCm3': '2.19639',
        'MaximumDensity1Cm2AreaDistanceFromSuperiorEdgeInMm': '104.7',
        'MaximumDensity1Cm2AreaDistanceFromPosteriorEdgeInMm': '47.4',
        'DenseAreaPercent': '88.8207',
        'AreaGreaterThan10mmDenseMm2': '2567.97',
        'HintVolumeCm3': '51.4824',
        'BreastVolumeCm3': '384.5103',
        'VolumetricBreastDensity': '13.3891',
        'Out_BreastVolume': '384.5',
        'Out_FGTV': '51.5',
        'Out_Density': '13.4',
        'Run_Information': 'VolparaVersion = 1.5.4.0 | 9561 |',
        'VolparaOkay': null
      },
      'OtherData': {
        'Projectcompletedsuccessfully': null,
        'PatientID': '0001',
        'FibroglandularTissueVolume': '51.5 cm3',
        'BreastVolume': '384.5 cm3',
        'VolumetricBreastDensity': '13.4 %',
        'RunInformation': 'VolparaVersion = 1.5.4.0 | 9561 |'
      },
      'SpecialData': null,
      'DicomFileMetaInfo': {
        '00020001': {
          'vr': 'OB',
          'InlineBinary': 'AAE='
        },
        '00020002': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.5.1.4.1.1.1.2.1'
          ]
        },
        '00020003': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401561177965'
          ]
        },
        '00020010': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.1.2.1'
          ]
        },
        '00020012': {
          'vr': 'UI',
          'Value': [
            '1.3.6.1.4.1.30071.8'
          ]
        },
        '00020013': {
          'vr': 'SH',
          'Value': [
            'fo-dicom 3.1.0'
          ]
        },
        '00020016': {
          'vr': 'AE',
          'Value': [
            'RD0003FF85A0E1'
          ]
        }
      },
      'DicomHeaderInfo': {
        '00080005': {
          'vr': 'CS',
          'Value': [
            'ISO_IR 100'
          ]
        },
        '00080008': {
          'vr': 'CS',
          'Value': [
            'ORIGINAL',
            'PRIMARY',
            null
          ]
        },
        '00080016': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.5.1.4.1.1.1.2.1'
          ]
        },
        '00080018': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401561177965'
          ]
        },
        '00080020': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080021': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080022': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080023': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080030': {
          'vr': 'TM',
          'Value': [
            '144815'
          ]
        },
        '00080031': {
          'vr': 'TM',
          'Value': [
            '145027'
          ]
        },
        '00080032': {
          'vr': 'TM',
          'Value': [
            '145148'
          ]
        },
        '00080033': {
          'vr': 'TM',
          'Value': [
            '145154'
          ]
        },
        '00080050': {
          'vr': 'SH',
          'Value': [
            '12345'
          ]
        },
        '00080060': {
          'vr': 'CS',
          'Value': [
            'MG'
          ]
        },
        '00080064': {
          'vr': 'CS',
          'Value': [
            'WSD'
          ]
        },
        '00080068': {
          'vr': 'CS',
          'Value': [
            'FOR PROCESSING'
          ]
        },
        '00080070': {
          'vr': 'LO',
          'Value': [
            'GE MEDICAL SYSTEMS'
          ]
        },
        '00080090': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Physician-1'
            }
          ]
        },
        '00081010': {
          'vr': 'SH',
          'Value': [
            'Station-1'
          ]
        },
        '00081030': {
          'vr': 'LO',
          'Value': [
            'B/L MLOS, B/L CCS'
          ]
        },
        '0008103E': {
          'vr': 'LO',
          'Value': [
            'B/L MLOS, B/L CCS'
          ]
        },
        '00081070': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Technologist-1'
            }
          ]
        },
        '00081090': {
          'vr': 'LO',
          'Value': [
            'Senograph DS ADS_43.10.1'
          ]
        },
        '00082218': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  'T-04000'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'SNM3'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'BREAST'
                ]
              }
            }
          ]
        },
        '00090010': {
          'vr': 'LO',
          'Value': [
            'SECTRA_Ident_01'
          ]
        },
        '00091001': {
          'vr': 'SH',
          'Value': [
            '0001223861'
          ]
        },
        '00091002': {
          'vr': 'SH',
          'Value': [
            '01'
          ]
        },
        '00100010': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Anonymous'
            }
          ]
        },
        '00100020': {
          'vr': 'LO',
          'Value': [
            '0001'
          ]
        },
        '00100030': {
          'vr': 'DA',
          'Value': [
            '19600701'
          ]
        },
        '00100040': {
          'vr': 'CS',
          'Value': [
            'F'
          ]
        },
        '00101010': {
          'vr': 'AS',
          'Value': [
            '049Y'
          ]
        },
        '00120062': {
          'vr': 'CS',
          'Value': [
            'YES'
          ]
        },
        '00120063': {
          'vr': 'LO',
          'Value': [
            'Basic Application Confidentiality Profile',
            'Clean Descriptors Option',
            'Retain Longitudinal Temporal Information Modified Dates Option',
            'Retain Patient Characteristics Option',
            'Retain Safe Private Option'
          ]
        },
        '00120064': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113100'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Basic Application Confidentiality Profile'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113105'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Clean Descriptors Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113107'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Longitudinal Temporal Information Modified Dates Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113108'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Patient Characteristics Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113111'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Safe Private Option'
                ]
              }
            }
          ]
        },
        '00180015': {
          'vr': 'CS',
          'Value': [
            'BREAST'
          ]
        },
        '00180060': {
          'vr': 'DS',
          'Value': [
            28
          ]
        },
        '00181000': {
          'vr': 'LO',
          'Value': [
            'SerialNo-1'
          ]
        },
        '00181020': {
          'vr': 'LO',
          'Value': [
            'Ads Application Package VERSION ADS_43.10.1'
          ]
        },
        '00181030': {
          'vr': 'LO',
          'Value': [
            'ROUTINE'
          ]
        },
        '00181110': {
          'vr': 'DS',
          'Value': [
            660
          ]
        },
        '00181111': {
          'vr': 'DS',
          'Value': [
            660
          ]
        },
        '00181114': {
          'vr': 'DS',
          'Value': [
            1
          ]
        },
        '00181147': {
          'vr': 'CS',
          'Value': [
            'RECTANGLE'
          ]
        },
        '00181149': {
          'vr': 'IS',
          'Value': [
            229,
            191
          ]
        },
        '00181150': {
          'vr': 'IS',
          'Value': [
            2684
          ]
        },
        '00181151': {
          'vr': 'IS',
          'Value': [
            62
          ]
        },
        '00181152': {
          'vr': 'IS',
          'Value': [
            166
          ]
        },
        '00181153': {
          'vr': 'IS',
          'Value': [
            166500
          ]
        },
        '00181160': {
          'vr': 'SH',
          'Value': [
            'STRIP'
          ]
        },
        '00181164': {
          'vr': 'DS',
          'Value': [
            0.094090909,
            0.094090909
          ]
        },
        '00181166': {
          'vr': 'CS',
          'Value': [
            'RECIPROCATING',
            'FOCUSED'
          ]
        },
        '00181190': {
          'vr': 'DS',
          'Value': [
            0.3
          ]
        },
        '00181191': {
          'vr': 'CS',
          'Value': [
            'RHODIUM'
          ]
        },
        '001811A0': {
          'vr': 'DS',
          'Value': [
            52
          ]
        },
        '001811A2': {
          'vr': 'DS',
          'Value': [
            110
          ]
        },
        '00181405': {
          'vr': 'IS',
          'Value': [
            14388
          ]
        },
        '00181508': {
          'vr': 'CS',
          'Value': [
            'MAMMOGRAPHIC'
          ]
        },
        '00181510': {
          'vr': 'DS',
          'Value': [
            -45
          ]
        },
        '00181531': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00181700': {
          'vr': 'CS',
          'Value': [
            'RECTANGULAR'
          ]
        },
        '00181702': {
          'vr': 'IS',
          'Value': [
            0
          ]
        },
        '00181704': {
          'vr': 'IS',
          'Value': [
            1915
          ]
        },
        '00181706': {
          'vr': 'IS',
          'Value': [
            0
          ]
        },
        '00181708': {
          'vr': 'IS',
          'Value': [
            2295
          ]
        },
        '00185101': {
          'vr': 'CS',
          'Value': [
            'MLO'
          ]
        },
        '00186000': {
          'vr': 'DS',
          'Value': [
            0.01117304
          ]
        },
        '00187000': {
          'vr': 'CS',
          'Value': [
            'YES'
          ]
        },
        '00187001': {
          'vr': 'DS',
          'Value': [
            30.299999
          ]
        },
        '00187004': {
          'vr': 'CS',
          'Value': [
            'SCINTILLATOR'
          ]
        },
        '00187005': {
          'vr': 'CS',
          'Value': [
            'AREA'
          ]
        },
        '00187006': {
          'vr': 'LT',
          'Value': [
            'DETECTOR VERSION 1.0 MTFCOMP 1.0'
          ]
        },
        '0018700A': {
          'vr': 'SH',
          'Value': [
            'DetectorID-1'
          ]
        },
        '0018700C': {
          'vr': 'DA',
          'Value': [
            '20080717'
          ]
        },
        '0018701A': {
          'vr': 'DS',
          'Value': [
            1,
            1
          ]
        },
        '00187020': {
          'vr': 'DS',
          'Value': [
            0.1,
            0.1
          ]
        },
        '00187022': {
          'vr': 'DS',
          'Value': [
            0.1,
            0.1
          ]
        },
        '00187024': {
          'vr': 'CS',
          'Value': [
            'RECTANGLE'
          ]
        },
        '00187026': {
          'vr': 'DS',
          'Value': [
            192,
            230.4
          ]
        },
        '00187030': {
          'vr': 'DS',
          'Value': [
            5,
            1
          ]
        },
        '00187032': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00187034': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00187050': {
          'vr': 'CS',
          'Value': [
            'RHODIUM'
          ]
        },
        '00187060': {
          'vr': 'CS',
          'Value': [
            'AUTOMATIC'
          ]
        },
        '00187062': {
          'vr': 'LT',
          'Value': [
            'AOP contrast RECTANGLE 1032 mm 430 mm 180 mm 240 mm EXP DOSE 146841 nGy PRE-EXP DOSE 3565 nGy PRE-EXP THICK 51 mm PRE-EXP COMPO 66 % PRE-EXP KV 28 PRE-EXP TRACK Rh PRE-EXP FILTER Rh PADDLE 0 FLATFIELD no'
          ]
        },
        '00187064': {
          'vr': 'CS',
          'Value': [
            'NORMAL'
          ]
        },
        '0018A001': {
          'vr': 'SQ',
          'Value': [
            {
              '00080070': {
                'vr': 'LO',
                'Value': [
                  'Matakina Technology'
                ]
              },
              '00081090': {
                'vr': 'LO',
                'Value': [
                  'Volpara Data Manager'
                ]
              },
              '00181020': {
                'vr': 'LO',
                'Value': [
                  '1.0.108.0'
                ]
              },
              '0018A002': {
                'vr': 'DT',
                'Value': [
                  '20151208'
                ]
              },
              '0018A003': {
                'vr': 'ST',
                'Value': [
                  'De-identifying Equipment'
                ]
              },
              '0040A170': {
                'vr': 'SQ',
                'Value': [
                  {
                    '00080100': {
                      'vr': 'SH',
                      'Value': [
                        '109104'
                      ]
                    },
                    '00080102': {
                      'vr': 'SH',
                      'Value': [
                        'DCM'
                      ]
                    },
                    '00080104': {
                      'vr': 'LO',
                      'Value': [
                        'De-identifying Equipment'
                      ]
                    }
                  }
                ]
              }
            }
          ]
        },
        '0020000D': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.1'
          ]
        },
        '0020000E': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733'
          ]
        },
        '00200010': {
          'vr': 'SH',
          'Value': [
            'Study-1'
          ]
        },
        '00200011': {
          'vr': 'IS',
          'Value': [
            4
          ]
        },
        '00200013': {
          'vr': 'IS',
          'Value': [
            3
          ]
        },
        '00200020': {
          'vr': 'CS',
          'Value': [
            'A',
            'FR'
          ]
        },
        '00200062': {
          'vr': 'CS',
          'Value': [
            'L'
          ]
        },
        '00280002': {
          'vr': 'US',
          'Value': [
            1
          ]
        },
        '00280004': {
          'vr': 'CS',
          'Value': [
            'MONOCHROME1'
          ]
        },
        '00280006': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00280008': {
          'vr': 'IS',
          'Value': [
            1
          ]
        },
        '00280010': {
          'vr': 'US',
          'Value': [
            2294
          ]
        },
        '00280011': {
          'vr': 'US',
          'Value': [
            1914
          ]
        },
        '00280100': {
          'vr': 'US',
          'Value': [
            16
          ]
        },
        '00280101': {
          'vr': 'US',
          'Value': [
            14
          ]
        },
        '00280102': {
          'vr': 'US',
          'Value': [
            13
          ]
        },
        '00280103': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00280300': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00280301': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00280303': {
          'vr': 'CS',
          'Value': [
            'MODIFIED'
          ]
        },
        '00281040': {
          'vr': 'CS',
          'Value': [
            'LIN'
          ]
        },
        '00281041': {
          'vr': 'SS',
          'Value': [
            1
          ]
        },
        '00281052': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00281053': {
          'vr': 'DS',
          'Value': [
            1
          ]
        },
        '00281054': {
          'vr': 'LO',
          'Value': [
            'US'
          ]
        },
        '00281300': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00282110': {
          'vr': 'CS',
          'Value': [
            '00'
          ]
        },
        '00290010': {
          'vr': 'LO',
          'Value': [
            'SECTRA_ImageInfo_01'
          ]
        },
        '00291004': {
          'vr': 'UN',
          'InlineBinary': 'dmlld19zdGF0ZSB7CiAgICBmaTwwPgogICAgc2k8MD4KICAgIGlpPDA+Cn0KZGVmYXVsdF9waXBlCnBpcGVfc3RhdGVzCnBpcGVfb3ZlcmxheXMKY29sbGVjdGlvbjxXOmV3YmN3aXNlOjQ0MTYyMjkgMD4Kb3JkZXI8MTY6ND4Kc29ydF9vcmRlcjxhc2NlbmRpbmc+CnNvcnRfb3BlcmF0aW9uPG5vbmU+CgAK'
        },
        '00400275': {
          'vr': 'SQ',
          'Value': [
            {
              '00400007': {
                'vr': 'LO',
                'Value': [
                  'B/L MLOS, B/L CCS'
                ]
              }
            }
          ]
        },
        '00400302': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00400306': {
          'vr': 'DS',
          'Value': [
            608
          ]
        },
        '00400310': {
          'vr': 'ST',
          'Value': [
            '66 %'
          ]
        },
        '00400316': {
          'vr': 'DS',
          'Value': [
            0.03076
          ]
        },
        '00400318': {
          'vr': 'CS',
          'Value': [
            'BREAST'
          ]
        },
        '00400555': {
          'vr': 'SQ'
        },
        '00408302': {
          'vr': 'DS',
          'Value': [
            14.388
          ]
        },
        '00450010': {
          'vr': 'LO',
          'Value': [
            'GEMS_SENO_02'
          ]
        },
        '00451006': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '0045101B': {
          'vr': 'CS',
          'Value': [
            'LMLO'
          ]
        },
        '00451020': {
          'vr': 'DS',
          'Value': [
            2073.0884
          ]
        },
        '00451026': {
          'vr': 'OB',
          'InlineBinary': 'ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgAA=='
        },
        '00451029': {
          'vr': 'DS',
          'Value': [
            1803,
            5.5
          ]
        },
        '0045102A': {
          'vr': 'IS',
          'Value': [
            -1
          ]
        },
        '0045102B': {
          'vr': 'IS',
          'Value': [
            -1
          ]
        },
        '00451049': {
          'vr': 'UN',
          'InlineBinary': 'NTE='
        },
        '00451050': {
          'vr': 'UN',
          'InlineBinary': 'MS4yLjg0MC4xMTM2MTkuMi42Ni4yMjAzODE2MTg4LjE0OTkwOTAxMzAxNDUxNTQuNjUwAA=='
        },
        '00451051': {
          'vr': 'UN',
          'InlineBinary': 'MS4yLjg0MC4xMTM2MTkuMi42Ni4yMjAzODE2MTg4LjIzMjUwMDkwMTMwMTQ1MDI3LjEwMDA2'
        },
        '00451071': {
          'vr': 'UN',
          'InlineBinary': 'ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgAA=='
        },
        '00451072': {
          'vr': 'UN',
          'InlineBinary': 'NVwxIA=='
        },
        '00540220': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  'R-10226'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'SNM3'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'medio-lateral oblique'
                ]
              },
              '00540222': {
                'vr': 'SQ'
              }
            }
          ]
        },
        '20500020': {
          'vr': 'CS',
          'Value': [
            'INVERSE'
          ]
        }
      },
      'ImageResultCreationTimeUtc': '2018-04-26T05:22:12.7589813Z'
    },
    {
      'JsonVersion': 1,
      'SourceImage': {
        'DicomImageFilePath': '0001_GE_R_rm_20090122.dcm',
        'TransferSyntaxName': 'Explicit VR Little Endian',
        'TransferSyntaxUid': '1.2.840.10008.1.2.1',
        'StudyInstanceUid': '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.1',
        'SopInstanceUid': '1.2.826.0.1.3680043.8.694.211979051425099.5028401647480257',
        'PixelSizeX': 0.094090909,
        'PixelSizeY': 0.094090909,
        'PixelRows': 2294,
        'PixelColumns': 1914,
        'JsonVersion': 1,
        'CryptoVersion': 'H1E1',
        'Hashes': [
          {
            'Key': 'Key1',
            'Value': 'PATIENTID'
          }
        ],
        'Encryptions': []
      },
      'MachineLearning': null,
      'PositioningInputs': null,
      'Scorecard': {
        'FibroglandularTissueVolumeInCm3': 44.7979,
        'BreastVolumeInCm3': 343.5815,
        'VolumetricBreastDensityInPercent': 13.0385,
        'ManufacturerDoseInmGy': 2.993,
        'VolparaDoseInmGy': 3.3264,
        'AppliedPressureInkPa': 12.45,
        'AppliedForceInN': 100.0,
        'JsonVersion': 1
      },
      'Calculations': null,
      'Algorithm': {
        'IsInputFileNotDicom': false,
        'IsAlgorithmSuccessful': true,
        'AlgorithmError': null,
        'AlgorithmRunInformation': 'VolparaVersion = 1.5.4.0 | 9561 |',
        'SourceDotOutFileName': null,
        'AlgorithmVersion': '1.5.4.0',
        'AlgorithmBuild': '9561',
        'JsonVersion': 1
      },
      'XlsData': {
        'MammoImageType': '2D',
        'RequestedProcedure': 'Unknown',
        'Folder': 'D:\\local\\Temp\\AlgorithmProcess\\imagecalc_636603168238658972_2',
        'Timestamp': '2018/04/26_05:20:41',
        'OSName': 'Unknown',
        'OSVersion': 'Unknown',
        'CurrentCulture': 'en-US',
        'InstalledUICulture': 'en-US',
        'WriteOutDisplayImage': 'Yes [ From Command Line ]',
        'VolparaVersion': '1.5.4.0 | 9561 |',
        'DICOMTAGManufacturer': 'GE',
        'DICOMTAGDeviceSerialNumber': 'SerialNo-1',
        'DICOMTagDetector_ID': 'DetectorID-1',
        'MaxAllowedKVP': '100 ( current 28 )',
        'BTSF': '1',
        'BTSTF': '0',
        'DetectorType': 'GE [ From Manufacturer ID ]',
        'DoFlatFieldCorrection': 'no [ From Manufacturer ID ]',
        'FSensitivity': '0.011173 [ From File ]',
        'Gain': '0.0100 [ From Manufacturer ID ]',
        'NativePixelSize': '0.1000 [ From Manufacturer ID ]',
        'Offset': '0.0000 [ From Manufacturer ID ]',
        'ScalePixelSize': 'yes [ From Manufacturer ID ]',
        'SourceToDetector': '660.0 [ From Manufacturer ID ]',
        'SupportToDetector': '40.0 [ From Manufacturer ID ]',
        'TubeType': 'GE [ From Manufacturer ID ]',
        'UseNewSlantAlgorithm': 'False [ From Calculations ]',
        'ValidToProcess': 'yes [ From Manufacturer ID ]',
        'BreastSide': 'Right',
        'ChestPosition': 'Right',
        'PectoralPosition': 'Top',
        'MammoView': 'MLO',
        'StudyDate': '20090122',
        'OperatorName': 'Technologist-1',
        'PatientDOB': '19600701',
        'PatientAge': '49',
        'PatientID': '0001',
        'DetectorID': 'DetectorID-1',
        'XraySystem': 'GE',
        'TargetPixelSizeMm': '0.355',
        'NearestNeighborResample': null,
        'ResizedPixelSizeMm': '0.3',
        'PectoralSide': 'Top',
        'PaddleType': 'None',
        'ExposureMas': '161',
        'ExposureTimeMs': '2590',
        'TargetMaterial': 'RHODIUM',
        'FilterMaterial': 'RHODIUM',
        'FilterThicknessMm': '0.025',
        'TubeVoltageKvp': '28',
        'CompressionPlateSlant': '5',
        'HVL_Mm': '0.414365',
        'CompressionForceN': '100',
        'RecordedBreastThicknessMm': '50',
        'PectoralAngleDegrees': '15.500',
        'PectoralAngleConfidence': '1.000',
        'InnerBreastStatistics': '( 22; 202; 51; 555; 36084; 78800482.000000; 177293300312.000000; 2183.806641; 379.917877 )',
        'muFatPerMm': '0.045',
        'MethodAllPlaneFit': '0.414584',
        'RejectingMethod1Reason': '-0.680866 > 10 or -0.680866 < -2 or 0.414584 < 0.85',
        'MethodFatPlaneFit': '0.881356',
        'Calculated_Sigma': '5',
        'ComputedSlantAngle': '1.05646',
        'ComputedSlantMm': '3.52956',
        'ComputedBreastThickness': '50',
        'ScatterScaleFactor': '1.00',
        'Scatter': 'Weighted',
        'SegPhaseDE': '29862.18',
        'SegPhaseOD': '0.00',
        'SegPhaseBE': '3794.40',
        'SegPhasePA': '5033.97',
        'SegPhaseBA': '5235.75',
        'SegPhaseOA': '0.00',
        'SegPhaseUA': '0.00',
        'SegPhasePD': '0.00',
        'SegSphereDE': '29862.18',
        'SegSphereOD': '0.00',
        'SegSphereBE': '5782.59',
        'SegSpherePA': '5033.97',
        'SegSphereBA': '3247.56',
        'SegSphereOA': '0.00',
        'SegSphereUA': '0.00',
        'SegSpherePD': '0.00',
        'ContactAreaMm2': '8031.62',
        'CompressionPressureKPa': '12.45',
        'PFAT_Edge_Zone': '0 0',
        'HintRejectLevel': '50.00 mm',
        'HintIgnoreLevel': '45.00 mm',
        'EntranceDoseInmGy': '13.773',
        'EstimatedEntranceDoseInmGy': '11.7499',
        'Warning': 'No HVL - using estimate',
        'GlandularityPercent': '33.58',
        'VolparaMeanGlandularDoseInmGy': '3.326400',
        'FiftyPercentGlandularDoseInmGy': '3.083375',
        'OrganDose': '0.029930',
        'OrganDoseInmGy': '2.993000',
        'Method2Results343.58144.797913.0385': 'APJ',
        'CorrectionComplete': null,
        'NippleConfidence': '0.996799',
        'NippleConfidenceMessage': '|OK||OK||OK|',
        'NippleInProfile': 'Yes',
        'NippleDistanceFromSuperiorEdgeInMm': '145.412',
        'NippleDistanceFromPosteriorEdgeInMm': '78.9076',
        'NippleCenterDistanceFromSuperiorEdgeInMm': '143.62',
        'NippleCenterDistanceFromPosteriorEdgeInMm': '75.7714',
        'MLOPosteriorNippleLineLengthInMm': '64.5619',
        'NippleLineLengthInMm': '78.9076',
        'PNLToInferiorPectoralMuscleVerticalLengthInMm': '-61.1571',
        'PectoralSkinFoldPresent': 'No',
        'NippleToInferiorPectoralMuscleVerticalLengthInMm': '-44.1883',
        'SuperiorPectoralWidthInMm': '53.1',
        'PosteriorPectoralLengthInMm': '189.6',
        'PectoralShape': 'CONCAVE',
        'ImfMaxDistanceMm': '17.5684',
        'InframammaryFoldVisible': 'Yes',
        'InframammaryFoldAreaInMm2': '744.57',
        'ImfAngleInDegrees': '137.158',
        'ImfSkinFoldPresent': 'No',
        'MeanDenseThicknessInMm': '12.6108',
        'MaximumDenseThicknessInMm': '25.5331',
        'SDDenseThicknessInMm': '4.83861',
        'MaximumDenseThicknessDistanceFromSuperiorEdgeInMm': '120.9',
        'MaximumDenseThicknessDistanceFromPosteriorEdgeInMm': '46.2',
        'DensityMapAttenuatingPixelCount': '0',
        'MaximumPercentDensityIn1Cm2Area': '40.2399',
        'MaximumDenseVolumeIn1Cm2AreaInCm3': '1.97196',
        'MaximumDensity1Cm2AreaDistanceFromSuperiorEdgeInMm': '120',
        'MaximumDensity1Cm2AreaDistanceFromPosteriorEdgeInMm': '42',
        'DenseAreaPercent': '94.2052',
        'AreaGreaterThan10mmDenseMm2': '2206.53',
        'HintVolumeCm3': '44.7979',
        'BreastVolumeCm3': '343.5815',
        'VolumetricBreastDensity': '13.0385',
        'Out_BreastVolume': '343.6',
        'Out_FGTV': '44.8',
        'Out_Density': '13.0',
        'Run_Information': 'VolparaVersion = 1.5.4.0 | 9561 |',
        'VolparaOkay': null
      },
      'OtherData': {
        'Projectcompletedsuccessfully': null,
        'PatientID': '0001',
        'FibroglandularTissueVolume': '44.8 cm3',
        'BreastVolume': '343.6 cm3',
        'VolumetricBreastDensity': '13.0 %',
        'RunInformation': 'VolparaVersion = 1.5.4.0 | 9561 |'
      },
      'SpecialData': null,
      'DicomFileMetaInfo': {
        '00020001': {
          'vr': 'OB',
          'InlineBinary': 'AAE='
        },
        '00020002': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.5.1.4.1.1.1.2.1'
          ]
        },
        '00020003': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401647480257'
          ]
        },
        '00020010': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.1.2.1'
          ]
        },
        '00020012': {
          'vr': 'UI',
          'Value': [
            '1.3.6.1.4.1.30071.8'
          ]
        },
        '00020013': {
          'vr': 'SH',
          'Value': [
            'fo-dicom 3.1.0'
          ]
        },
        '00020016': {
          'vr': 'AE',
          'Value': [
            'RD0003FF85A0E1'
          ]
        }
      },
      'DicomHeaderInfo': {
        '00080005': {
          'vr': 'CS',
          'Value': [
            'ISO_IR 100'
          ]
        },
        '00080008': {
          'vr': 'CS',
          'Value': [
            'ORIGINAL',
            'PRIMARY',
            null
          ]
        },
        '00080016': {
          'vr': 'UI',
          'Value': [
            '1.2.840.10008.5.1.4.1.1.1.2.1'
          ]
        },
        '00080018': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401647480257'
          ]
        },
        '00080020': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080021': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080022': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080023': {
          'vr': 'DA',
          'Value': [
            '20090122'
          ]
        },
        '00080030': {
          'vr': 'TM',
          'Value': [
            '144815'
          ]
        },
        '00080031': {
          'vr': 'TM',
          'Value': [
            '145027'
          ]
        },
        '00080032': {
          'vr': 'TM',
          'Value': [
            '145240'
          ]
        },
        '00080033': {
          'vr': 'TM',
          'Value': [
            '145245'
          ]
        },
        '00080050': {
          'vr': 'SH',
          'Value': [
            '12345'
          ]
        },
        '00080060': {
          'vr': 'CS',
          'Value': [
            'MG'
          ]
        },
        '00080064': {
          'vr': 'CS',
          'Value': [
            'WSD'
          ]
        },
        '00080068': {
          'vr': 'CS',
          'Value': [
            'FOR PROCESSING'
          ]
        },
        '00080070': {
          'vr': 'LO',
          'Value': [
            'GE MEDICAL SYSTEMS'
          ]
        },
        '00080090': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Physician-1'
            }
          ]
        },
        '00081010': {
          'vr': 'SH',
          'Value': [
            'Station-1'
          ]
        },
        '00081030': {
          'vr': 'LO',
          'Value': [
            'B/L MLOS, B/L CCS'
          ]
        },
        '0008103E': {
          'vr': 'LO',
          'Value': [
            'B/L MLOS, B/L CCS'
          ]
        },
        '00081070': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Technologist-1'
            }
          ]
        },
        '00081090': {
          'vr': 'LO',
          'Value': [
            'Senograph DS ADS_43.10.1'
          ]
        },
        '00082218': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  'T-04000'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'SNM3'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'BREAST'
                ]
              }
            }
          ]
        },
        '00090010': {
          'vr': 'LO',
          'Value': [
            'SECTRA_Ident_01'
          ]
        },
        '00091001': {
          'vr': 'SH',
          'Value': [
            '0001223861'
          ]
        },
        '00091002': {
          'vr': 'SH',
          'Value': [
            '01'
          ]
        },
        '00100010': {
          'vr': 'PN',
          'Value': [
            {
              'Alphabetic': 'Anonymous'
            }
          ]
        },
        '00100020': {
          'vr': 'LO',
          'Value': [
            '0001'
          ]
        },
        '00100030': {
          'vr': 'DA',
          'Value': [
            '19600701'
          ]
        },
        '00100040': {
          'vr': 'CS',
          'Value': [
            'F'
          ]
        },
        '00101010': {
          'vr': 'AS',
          'Value': [
            '049Y'
          ]
        },
        '00120062': {
          'vr': 'CS',
          'Value': [
            'YES'
          ]
        },
        '00120063': {
          'vr': 'LO',
          'Value': [
            'Basic Application Confidentiality Profile',
            'Clean Descriptors Option',
            'Retain Longitudinal Temporal Information Modified Dates Option',
            'Retain Patient Characteristics Option',
            'Retain Safe Private Option'
          ]
        },
        '00120064': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113100'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Basic Application Confidentiality Profile'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113105'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Clean Descriptors Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113107'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Longitudinal Temporal Information Modified Dates Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113108'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Patient Characteristics Option'
                ]
              }
            },
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  '113111'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'DCM'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'Retain Safe Private Option'
                ]
              }
            }
          ]
        },
        '00180015': {
          'vr': 'CS',
          'Value': [
            'BREAST'
          ]
        },
        '00180060': {
          'vr': 'DS',
          'Value': [
            28
          ]
        },
        '00181000': {
          'vr': 'LO',
          'Value': [
            'SerialNo-1'
          ]
        },
        '00181020': {
          'vr': 'LO',
          'Value': [
            'Ads Application Package VERSION ADS_43.10.1'
          ]
        },
        '00181030': {
          'vr': 'LO',
          'Value': [
            'ROUTINE'
          ]
        },
        '00181110': {
          'vr': 'DS',
          'Value': [
            660
          ]
        },
        '00181111': {
          'vr': 'DS',
          'Value': [
            660
          ]
        },
        '00181114': {
          'vr': 'DS',
          'Value': [
            1
          ]
        },
        '00181147': {
          'vr': 'CS',
          'Value': [
            'RECTANGLE'
          ]
        },
        '00181149': {
          'vr': 'IS',
          'Value': [
            229,
            191
          ]
        },
        '00181150': {
          'vr': 'IS',
          'Value': [
            2590
          ]
        },
        '00181151': {
          'vr': 'IS',
          'Value': [
            62
          ]
        },
        '00181152': {
          'vr': 'IS',
          'Value': [
            161
          ]
        },
        '00181153': {
          'vr': 'IS',
          'Value': [
            160700
          ]
        },
        '00181160': {
          'vr': 'SH',
          'Value': [
            'STRIP'
          ]
        },
        '00181164': {
          'vr': 'DS',
          'Value': [
            0.094090909,
            0.094090909
          ]
        },
        '00181166': {
          'vr': 'CS',
          'Value': [
            'RECIPROCATING',
            'FOCUSED'
          ]
        },
        '00181190': {
          'vr': 'DS',
          'Value': [
            0.3
          ]
        },
        '00181191': {
          'vr': 'CS',
          'Value': [
            'RHODIUM'
          ]
        },
        '001811A0': {
          'vr': 'DS',
          'Value': [
            50
          ]
        },
        '001811A2': {
          'vr': 'DS',
          'Value': [
            100
          ]
        },
        '00181405': {
          'vr': 'IS',
          'Value': [
            13773
          ]
        },
        '00181508': {
          'vr': 'CS',
          'Value': [
            'MAMMOGRAPHIC'
          ]
        },
        '00181510': {
          'vr': 'DS',
          'Value': [
            45
          ]
        },
        '00181531': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00181700': {
          'vr': 'CS',
          'Value': [
            'RECTANGULAR'
          ]
        },
        '00181702': {
          'vr': 'IS',
          'Value': [
            0
          ]
        },
        '00181704': {
          'vr': 'IS',
          'Value': [
            1915
          ]
        },
        '00181706': {
          'vr': 'IS',
          'Value': [
            0
          ]
        },
        '00181708': {
          'vr': 'IS',
          'Value': [
            2295
          ]
        },
        '00185101': {
          'vr': 'CS',
          'Value': [
            'MLO'
          ]
        },
        '00186000': {
          'vr': 'DS',
          'Value': [
            0.01117304
          ]
        },
        '00187000': {
          'vr': 'CS',
          'Value': [
            'YES'
          ]
        },
        '00187001': {
          'vr': 'DS',
          'Value': [
            30.299999
          ]
        },
        '00187004': {
          'vr': 'CS',
          'Value': [
            'SCINTILLATOR'
          ]
        },
        '00187005': {
          'vr': 'CS',
          'Value': [
            'AREA'
          ]
        },
        '00187006': {
          'vr': 'LT',
          'Value': [
            'DETECTOR VERSION 1.0 MTFCOMP 1.0'
          ]
        },
        '0018700A': {
          'vr': 'SH',
          'Value': [
            'DetectorID-1'
          ]
        },
        '0018700C': {
          'vr': 'DA',
          'Value': [
            '20080717'
          ]
        },
        '0018701A': {
          'vr': 'DS',
          'Value': [
            1,
            1
          ]
        },
        '00187020': {
          'vr': 'DS',
          'Value': [
            0.1,
            0.1
          ]
        },
        '00187022': {
          'vr': 'DS',
          'Value': [
            0.1,
            0.1
          ]
        },
        '00187024': {
          'vr': 'CS',
          'Value': [
            'RECTANGLE'
          ]
        },
        '00187026': {
          'vr': 'DS',
          'Value': [
            192,
            230.4
          ]
        },
        '00187030': {
          'vr': 'DS',
          'Value': [
            5,
            1
          ]
        },
        '00187032': {
          'vr': 'DS',
          'Value': [
            180
          ]
        },
        '00187034': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00187050': {
          'vr': 'CS',
          'Value': [
            'RHODIUM'
          ]
        },
        '00187060': {
          'vr': 'CS',
          'Value': [
            'AUTOMATIC'
          ]
        },
        '00187062': {
          'vr': 'LT',
          'Value': [
            'AOP contrast RECTANGLE 1032 mm 370 mm 180 mm 240 mm EXP DOSE 148983 nGy PRE-EXP DOSE 2935 nGy PRE-EXP THICK 51 mm PRE-EXP COMPO 73 % PRE-EXP KV 27 PRE-EXP TRACK Rh PRE-EXP FILTER Rh PADDLE 0 FLATFIELD no'
          ]
        },
        '00187064': {
          'vr': 'CS',
          'Value': [
            'NORMAL'
          ]
        },
        '0018A001': {
          'vr': 'SQ',
          'Value': [
            {
              '00080070': {
                'vr': 'LO',
                'Value': [
                  'Matakina Technology'
                ]
              },
              '00081090': {
                'vr': 'LO',
                'Value': [
                  'Volpara Data Manager'
                ]
              },
              '00181020': {
                'vr': 'LO',
                'Value': [
                  '1.0.108.0'
                ]
              },
              '0018A002': {
                'vr': 'DT',
                'Value': [
                  '20151208'
                ]
              },
              '0018A003': {
                'vr': 'ST',
                'Value': [
                  'De-identifying Equipment'
                ]
              },
              '0040A170': {
                'vr': 'SQ',
                'Value': [
                  {
                    '00080100': {
                      'vr': 'SH',
                      'Value': [
                        '109104'
                      ]
                    },
                    '00080102': {
                      'vr': 'SH',
                      'Value': [
                        'DCM'
                      ]
                    },
                    '00080104': {
                      'vr': 'LO',
                      'Value': [
                        'De-identifying Equipment'
                      ]
                    }
                  }
                ]
              }
            }
          ]
        },
        '0020000D': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733.1'
          ]
        },
        '0020000E': {
          'vr': 'UI',
          'Value': [
            '1.2.826.0.1.3680043.8.694.211979051425099.5028401519171733'
          ]
        },
        '00200010': {
          'vr': 'SH',
          'Value': [
            'Study-1'
          ]
        },
        '00200011': {
          'vr': 'IS',
          'Value': [
            3
          ]
        },
        '00200013': {
          'vr': 'IS',
          'Value': [
            4
          ]
        },
        '00200020': {
          'vr': 'CS',
          'Value': [
            'P',
            'FL'
          ]
        },
        '00200062': {
          'vr': 'CS',
          'Value': [
            'R'
          ]
        },
        '00280002': {
          'vr': 'US',
          'Value': [
            1
          ]
        },
        '00280004': {
          'vr': 'CS',
          'Value': [
            'MONOCHROME1'
          ]
        },
        '00280006': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00280008': {
          'vr': 'IS',
          'Value': [
            1
          ]
        },
        '00280010': {
          'vr': 'US',
          'Value': [
            2294
          ]
        },
        '00280011': {
          'vr': 'US',
          'Value': [
            1914
          ]
        },
        '00280100': {
          'vr': 'US',
          'Value': [
            16
          ]
        },
        '00280101': {
          'vr': 'US',
          'Value': [
            14
          ]
        },
        '00280102': {
          'vr': 'US',
          'Value': [
            13
          ]
        },
        '00280103': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00280300': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00280301': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00280303': {
          'vr': 'CS',
          'Value': [
            'MODIFIED'
          ]
        },
        '00281040': {
          'vr': 'CS',
          'Value': [
            'LIN'
          ]
        },
        '00281041': {
          'vr': 'SS',
          'Value': [
            1
          ]
        },
        '00281052': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '00281053': {
          'vr': 'DS',
          'Value': [
            1
          ]
        },
        '00281054': {
          'vr': 'LO',
          'Value': [
            'US'
          ]
        },
        '00281300': {
          'vr': 'CS',
          'Value': [
            'NO'
          ]
        },
        '00282110': {
          'vr': 'CS',
          'Value': [
            '00'
          ]
        },
        '00290010': {
          'vr': 'LO',
          'Value': [
            'SECTRA_ImageInfo_01'
          ]
        },
        '00291004': {
          'vr': 'UN',
          'InlineBinary': 'dmlld19zdGF0ZSB7CiAgICBmaTwwPgogICAgc2k8MD4KICAgIGlpPDA+Cn0KZGVmYXVsdF9waXBlCnBpcGVfc3RhdGVzCnBpcGVfb3ZlcmxheXMKY29sbGVjdGlvbjxXOmV3YmN3aXNlOjQ0MTYyMjUgMD4Kb3JkZXI8MTY6Mz4Kc29ydF9vcmRlcjxhc2NlbmRpbmc+CnNvcnRfb3BlcmF0aW9uPG5vbmU+CgAK'
        },
        '00400275': {
          'vr': 'SQ',
          'Value': [
            {
              '00400007': {
                'vr': 'LO',
                'Value': [
                  'B/L MLOS, B/L CCS'
                ]
              }
            }
          ]
        },
        '00400302': {
          'vr': 'US',
          'Value': [
            0
          ]
        },
        '00400306': {
          'vr': 'DS',
          'Value': [
            610
          ]
        },
        '00400310': {
          'vr': 'ST',
          'Value': [
            '72 %'
          ]
        },
        '00400316': {
          'vr': 'DS',
          'Value': [
            0.02993
          ]
        },
        '00400318': {
          'vr': 'CS',
          'Value': [
            'BREAST'
          ]
        },
        '00400555': {
          'vr': 'SQ'
        },
        '00408302': {
          'vr': 'DS',
          'Value': [
            13.773
          ]
        },
        '00450010': {
          'vr': 'LO',
          'Value': [
            'GEMS_SENO_02'
          ]
        },
        '00451006': {
          'vr': 'DS',
          'Value': [
            0
          ]
        },
        '0045101B': {
          'vr': 'CS',
          'Value': [
            'RMLO'
          ]
        },
        '00451020': {
          'vr': 'DS',
          'Value': [
            2161.1917
          ]
        },
        '00451026': {
          'vr': 'OB',
          'InlineBinary': 'ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgAA=='
        },
        '00451029': {
          'vr': 'DS',
          'Value': [
            1880,
            5.5
          ]
        },
        '0045102A': {
          'vr': 'IS',
          'Value': [
            -1
          ]
        },
        '0045102B': {
          'vr': 'IS',
          'Value': [
            -1
          ]
        },
        '00451049': {
          'vr': 'UN',
          'InlineBinary': 'NTE='
        },
        '00451050': {
          'vr': 'UN',
          'InlineBinary': 'MS4yLjg0MC4xMTM2MTkuMi42Ni4yMjAzODE2MTg4LjE0OTkwOTAxMzAxNDUyNDUuNjU0AA=='
        },
        '00451051': {
          'vr': 'UN',
          'InlineBinary': 'MS4yLjg0MC4xMTM2MTkuMi42Ni4yMjAzODE2MTg4LjIzMjUwMDkwMTMwMTQ1MDI3LjEwMDA2'
        },
        '00451071': {
          'vr': 'UN',
          'InlineBinary': 'ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgAA=='
        },
        '00451072': {
          'vr': 'UN',
          'InlineBinary': 'NVw1IA=='
        },
        '00540220': {
          'vr': 'SQ',
          'Value': [
            {
              '00080100': {
                'vr': 'SH',
                'Value': [
                  'R-10226'
                ]
              },
              '00080102': {
                'vr': 'SH',
                'Value': [
                  'SNM3'
                ]
              },
              '00080104': {
                'vr': 'LO',
                'Value': [
                  'medio-lateral oblique'
                ]
              },
              '00540222': {
                'vr': 'SQ'
              }
            }
          ]
        },
        '20500020': {
          'vr': 'CS',
          'Value': [
            'INVERSE'
          ]
        }
      },
      'ImageResultCreationTimeUtc': '2018-04-26T05:22:12.8964937Z'
    }
  ]
}";



            if (ModelState.IsValid)
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == model.NHSNumber).FirstOrDefault();
                if (participant != null)
                {
                    _exampleMessage = _exampleMessage.Replace("PATIENTID", participant.HashedNHSNumber);
                    List<string> results = _volparaService.ProcessScreeningMessage(_exampleMessage);
                    if (results.Count == 0)
                    {
                        return RedirectToAction("Details", "Participant", new { participantId = model.NHSNumber });
                    }
                    else
                    {
                        foreach (string result in results)
                        {
                            ModelState.AddModelError("NHSNumber", result);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("NHSNumber", "Participant does not exist");
                }
            }


            return View("ReceiveConsent", model);
        }
    }
}