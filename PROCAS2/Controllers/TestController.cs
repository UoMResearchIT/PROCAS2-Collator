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

        public TestController(IWebJobParticipantService participantService,
                                IGenericRepository<Participant> participantRepo,
                                ICRAService craService)
        {
            _participantService = participantService;
            _participantRepo = participantRepo;
            _craService = craService;
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
                        return RedirectToAction("Details", "Participant", new { participantId=model.NHSNumber });
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
    }
}