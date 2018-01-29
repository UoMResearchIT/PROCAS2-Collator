using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Services.Utility;

namespace PROCAS2.Controllers
{
#if RELEASE
    [RequireHttps]
#endif
    [Authorize]
    /// <summary>
    /// TODO: Remove this controller! Just used for testing
    /// </summary>
    public class CRAController : Controller
    {
        private ICRAService _hl7Service;
        public CRAController(ICRAService hl7Service)
        {
            _hl7Service = hl7Service;
        }


        private string _exampleMessage = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||10000:eQ0EeJI/cWdXvjLrn3CbD1R4CvdMCKl3hei9/7Z6yT5kdML/cJLiMK79vk3scHEAe9X5oeBZd2RTbk8kMt/SdpNBCo11A2NK2cSFnkRZ/Ri+C6f2dLO/T8hiyNznRtpa5lJbiQFhWHk4mApesUl/8K1eIPojj9L8ftk9bJ5V0/E=:h8VhQbPiVbC3qNOG5GRKhUkpMzDpmoizJPlrTzBbhut14a3SmHDfwy+GEjfALsMx9d6ACGiXd3A0+JdbhGtLDZ1+anIZ87dKAdCP1Ajx/cClLXJLmzM7kiNyFYXnoDPFuenuttdtJN8L3yZnN75PXRhkO7Q5XxAxcgab6kdtH20=||TestSuperShortBreast^Stephanie||19700923|F
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
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|33|ED|900^Risk Assessment Document^CRA|Survey Summary|^^PDF^base64^......||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";


        private string _otherMessage = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||99908081701||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|||||||||123^Walker^Jane|||||||||F
OBX|1|TX|100^Risk Assessment Summary^CRA||**********BREAST CANCER RISK ASSESSMENT SUMMARY********** ~Risk Assessment results are provided by CRA Health, Inc. ~Your patient completed a breast cancer risk assessment survey as part of her breast health screening. Based on the information she provided, her calculated risk of developing breast cancer is at a higher than average lifetime risk and/or hereditary predisposition to cancer. ~High risk patients, especially those with dense breasts, will benefit from additional clinical management and annual Breast MRI. ~Estimated Breast Cancer Risk Calculations: ~Risk BRCA1/2 Mutation: 1.5% ~Lifetime Breast Cancer Risk: 35.4% ~*ACS Guidelines: Patients at high risk will have a >= 10% risk of BRCA mutation and/or >= 20% lifetime risk of breast cancer ~*This analysis is only as accurate as the data entered or provided by the patient. ~Date of the Survey Calculations: Oct 30 2017 9:50PM~||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire. Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer. All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent. Key criteria for increased risk are: ~ - Personal or family history of breast and/or ovarian cancer~ - History of abnormal breast pathology~ - Personal history of chest radiation~ - Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~ - Mother: Breast Cancer age 65~ - Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|3|TX|120^Screening Mammography Provider Letter^CRA||Your patient completed a cancer risk assessment questionnaire during a visit to our screening facility. She provided the followinginformation regarding their personal and family history: ~ -Mother: Breast Cancer age 65~ - Sister: Breast Cancer age 55~The preliminary information your patient provided shows that she meets the criteria of the American Cancer Society Guidelines1 and the National Cancer Center Network (NCCN)2 Guidelines for screening MRI of the breast. Both guidelines recommend MRI of the breast if the patient has a lifetime risk of breast cancer of 20% or greater based on the Tyrer Cuzick, BRCAPRO or Claus Model.Your patient's risk is elevated by an accepted model:~ - Tyrer Cuzick (v8) 35.4% lifetime risk~Thus, she is considered at significantly increased risk of developing breast cancer as compared to other women her age. MRI would increase the chance of finding breast cancer at an earlier, more treatable stage. We recommend that you order this study on an annual basis. This would be in addition to her routine annual screening mammography.~If you agree, we suggest you order a breast screening MRI. The reason for the test should be listed as 'Lifetime Risk of breast cancer of 20% or greater by an accepted model'.~We have suggested that the patient talk with you about this if there are any concerns.~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available. Thank you for letting us participate in the care of your patients.~||||||F
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
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN~maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|33|ED|900^Risk Assessment Document^CRA|Survey Summary|^^PDF^base64^......||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";
        // GET: CRA
        public void Index()
        {
            _hl7Service.ProcessQuestionnaire(_exampleMessage);
        }


        public void PostServiceBus()
        {
            _hl7Service.PostServiceBusMessage(_exampleMessage);
        }


        public void GetServiceBus()
        {
            string message = _hl7Service.GetServiceBusMessage();
            _hl7Service.ProcessQuestionnaire(message);
        }
    
    }
}