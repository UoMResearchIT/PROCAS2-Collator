using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Mocks;
using NUnit.Framework;

using PROCAS2.Resources;
using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Services.Utility;

namespace PROCAS2.Tests.ServiceTests
{
    [TestFixture]
    public class CRAServiceTests:BaseServiceTest
    {
        [SetUp]
        public void SetUp()
        {
            SetUpMocks();
            SetUpStubs();
           
        }

        [TearDown]
        public void TearDown()
        {
            ClearMocks();
        }

        private CRAService CreateService()
        {
            return new CRAService(_participantService, _responseService, _configService, _logger);
        }

        [Test]
        public void IsConsent_Valid_Consent_Message_Returns_True()
        {
            string message = @"{ 'messageType' : 'consent', 'messageTimestamp' : '2018 - 01 - 15T14: 33:23', 'patientId' : '12345678' }";

            CRAService service = CreateService();
            bool ret = service.IsConsentMessage(message);

            Assert.AreEqual(true, ret);
        }

        [Test]
        public void IsConsent_Invalid_Consent_Message_Returns_False()
        {
            string message = @"{ 'wibble' : 'invalid', 'hello' : '2018 - 01 - 15T14: 33:23', 'wobble' : '12345678' }";

            CRAService service = CreateService();
            bool ret = service.IsConsentMessage(message);

            Assert.AreEqual(false, ret);
        }

        [Test]
        public void IsConsent_Consent_Message_No_JSON_Returns_False()
        {
            string message = @"Hi there, this is a string";

            CRAService service = CreateService();
            bool ret = service.IsConsentMessage(message);

            Assert.AreEqual(false, ret);
        }

        [Test]
        public void ProcessConsent_Invalid_Consent_Message_Returns_Error()
        {
            string message = @"{ 'wibble' : 'invalid', 'hello' : '2018 - 01 - 15T14: 33:23', 'wobble' : '12345678' }";
            string errorString = HL7Resources.CONSENT_MESSAGE_FORMAT_INVALID;
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");

            CRAService service = CreateService();
            List<string> ret = service.ProcessConsent(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);
        }

        [Test]
        public void ProcessConsent_Consent_Message_Not_JSON_Returns_Error()
        {
            string message = @"Hi there, this is a string";
            string errorString = HL7Resources.CONSENT_MESSAGE_FORMAT_INVALID;
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");

            CRAService service = CreateService();
            List<string> ret = service.ProcessConsent(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);
        }

        [Test]
        public void ProcessConsent_Invalid_PatientID_Returns_Error()
        {
            string message = @"{ 'messageType' : 'consent', 'messageTimestamp' : '2018 - 01 - 15T14: 33:23', 'patientId' : '1234' }";
            string errorString = String.Format(HL7Resources.PATIENT_NOT_EXISTS, "1234");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("1234")).Return(false);
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");

            CRAService service = CreateService();
            List<string> ret = service.ProcessConsent(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);
        }

        [Test]
        public void ProcessConsent_Error_Setting_Consent_Returns_Error()
        {
            string message = @"{ 'messageType' : 'consent', 'messageTimestamp' : '2018 - 01 - 15T14: 33:23', 'patientId' : '1234' }";
            string errorString = String.Format(HL7Resources.CONSENT_NOT_SET, "1234");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("1234")).Return(true);
            _participantService.Stub(x => x.SetConsentFlag("1234")).Return(false);
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");

            CRAService service = CreateService();
            List<string> ret = service.ProcessConsent(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);
        }

        [Test]
        public void ProcessConsent_Everything_OK_Returns_Nowt()
        {
            string message = @"{ 'messageType' : 'consent', 'messageTimestamp' : '2018 - 01 - 15T14: 33:23', 'patientId' : '1234' }";
            string errorString = String.Format(HL7Resources.CONSENT_NOT_SET, "1234");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("1234")).Return(true);
            _participantService.Stub(x => x.SetConsentFlag("1234")).Return(true);
            

            CRAService service = CreateService();
            List<string> ret = service.ProcessConsent(message);

            Assert.AreEqual(0, ret.Count);
           
        }

        [Test]
        public void ProcessQuestionnaire_Not_HL7_Returns_Error()
        {
            string message = @"{ 'messageType' : 'consent', 'messageTimestamp' : '2018 - 01 - 15T14: 33:23', 'patientId' : '1234' }";
            string errorString = HL7Resources.NOT_ORUR01;
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");


            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }

        [Test]
        public void ProcessQuestionnaire_HL7_Not_ORUR01_Returns_Error()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORC^R01|99900000001|P|2.3||||AL
PID|1||PATIENTID||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire.Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer.All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent.Key criteria for increased risk are: ~-Personal or family history of breast and / or ovarian cancer~-History of abnormal breast pathology~-Personal history of chest radiation~-Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = HL7Resources.NOT_ORUR01;
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");


            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }

        [Test]
        public void ProcessQuestionnaire_HL7_No_PatientID_Returns_Error()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire.Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer.All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent.Key criteria for increased risk are: ~-Personal or family history of breast and / or ovarian cancer~-History of abnormal breast pathology~-Personal history of chest radiation~-Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = String.Format(HL7Resources.PATIENT_NOT_EXISTS, "");
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");


            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }

        [Test]
        public void ProcessQuestionnaire_HL7_No_PID_Segment_Returns_Error()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire.Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer.All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent.Key criteria for increased risk are: ~-Personal or family history of breast and / or ovarian cancer~-History of abnormal breast pathology~-Personal history of chest radiation~-Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = String.Format(HL7Resources.PATIENT_NOT_EXISTS, "");
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");


            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }

        [Test]
        public void ProcessQuestionnaire_HL7_Invalid_PatientID_Returns_Error()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||PATIENTID||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire.Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer.All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent.Key criteria for increased risk are: ~-Personal or family history of breast and / or ovarian cancer~-History of abnormal breast pathology~-Personal history of chest radiation~-Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = String.Format(HL7Resources.PATIENT_NOT_EXISTS, "PATIENTID"); ;
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("PATIENTID")).Return(false);

            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }

        [Test]
        public void ProcessQuestionnaire_HL7_Cannot_Create_Questionnaire_Returns_Error()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||PATIENTID||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire.Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer.All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent.Key criteria for increased risk are: ~-Personal or family history of breast and / or ovarian cancer~-History of abnormal breast pathology~-Personal history of chest radiation~-Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = HL7Resources.HEADER_CREATION_ERROR;
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("PATIENTID")).Return(true);
            QuestionnaireResponse response;
            _responseService.Stub(x => x.CreateQuestionnaireHeader("PATIENTID", "20170808080023", "20170808080023", out response)).Return(false);

            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }

        [Test]
        public void ProcessQuestionnaire_HL7_Cannot_Create_FamilyHistory_Returns_Error()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||PATIENTID||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire.Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer.All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent.Key criteria for increased risk are: ~-Personal or family history of breast and / or ovarian cancer~-History of abnormal breast pathology~-Personal history of chest radiation~-Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = String.Format(HL7Resources.FAMILY_HISTORY_ERROR, "PATIENTID");
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("PATIENTID")).Return(true);
            QuestionnaireResponse response = new QuestionnaireResponse() {  };
            QuestionnaireResponse resp2;
            _responseService.Stub(x => x.CreateQuestionnaireHeader("PATIENTID", "20170808080023", "20170808080023", out resp2)).OutRef(response).Return(true);

            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode=="NFTH"))).Return(false);
            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode != "NFTH"))).Return(true);

            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }

        [Test]
        public void ProcessQuestionnaire_HL7_Cannot_Set_Consent_Returns_Error()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||PATIENTID||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire.Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer.All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent.Key criteria for increased risk are: ~-Personal or family history of breast and / or ovarian cancer~-History of abnormal breast pathology~-Personal history of chest radiation~-Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = String.Format(HL7Resources.CONSENT_NOT_SET, "PATIENTID");
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("PATIENTID")).Return(true);
            QuestionnaireResponse response = new QuestionnaireResponse() { };
            QuestionnaireResponse resp2;
            _responseService.Stub(x => x.CreateQuestionnaireHeader("PATIENTID", "20170808080023", "20170808080023", out resp2)).OutRef(response).Return(true);

            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode == "NFTH"))).Return(false);
            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode != "NFTH"))).Return(true);

            _participantService.Stub(x => x.SetConsentFlag("PATIENTID")).Return(false);

            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }


        [Test]
        public void ProcessQuestionnaire_HL7_Cannot_Create_Response_Item_Returns_Error()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||PATIENTID||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Thank you for taking time during your visit to complete the Breast Cancer Risk Assessment Questionnaire.Through this assessment, we strive to help identify women at higher than average risk of developing breast cancer.All information gathered during the assessment is strictly confidential and is never shared with any third party without your prior consent.Key criteria for increased risk are: ~-Personal or family history of breast and / or ovarian cancer~-History of abnormal breast pathology~-Personal history of chest radiation~-Family member with a known genetic mutation~~You provided the following information regarding your personal and family history:~-Mother: Breast Cancer age 65~-Sister: Breast Cancer age 55~~Our goal is to empower, educate, care for and support patients by providing them the most current and relevant information available.~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = String.Format(HL7Resources.OBX_ERROR, "1000.surveyQuestion1");
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("PATIENTID")).Return(true);
            QuestionnaireResponse response = new QuestionnaireResponse() { };
            QuestionnaireResponse resp2;
            _responseService.Stub(x => x.CreateQuestionnaireHeader("PATIENTID", "20170808080023", "20170808080023", out resp2)).OutRef(response).Return(true);

            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode == "NFTH"))).Return(false);
            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode != "NFTH"))).Return(true);

            _participantService.Stub(x => x.SetConsentFlag("PATIENTID")).Return(true);

            _responseService.Stub(x => x.CreateResponseItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<string>.Is.Equal("surveyQuestion1"), Arg<string>.Is.Equal("Survey Answer 1"))).Return(false);

            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }

        [Test]
        public void ProcessQuestionnaire_HL7_Cannot_Create_Risk_Letter_Returns_Error()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||PATIENTID||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Hello~Test~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|29|CE|400^Lifetime Breast Cancer Risk Category^CRA||HIGH||||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = String.Format(HL7Resources.RISK_LETTER_NOT_CREATED, "PATIENTID");
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("PATIENTID")).Return(true);
            QuestionnaireResponse response = new QuestionnaireResponse() { };
            QuestionnaireResponse resp2;
            _responseService.Stub(x => x.CreateQuestionnaireHeader("PATIENTID", "20170808080023", "20170808080023", out resp2)).OutRef(response).Return(true);

            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode == "NFTH"))).Return(false);
            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode != "NFTH"))).Return(true);

            _participantService.Stub(x => x.SetConsentFlag("PATIENTID")).Return(true);

            _responseService.Stub(x => x.CreateResponseItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<string>.Is.Equal("surveyQuestion1"), Arg<string>.Is.Equal("Survey Answer 1"))).Return(true);

            _participantService.Stub(x => x.CreateRiskLetter("PATIENTID", "29.01", "HIGH", new List<string>() {"Hello", "Test", "" })).Return(false);


            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual("Error", ret[0]);

        }

        [Test]
        public void ProcessQuestionnaire_HL7_No_Problems_Returns_Nowt()
        {
            string message = @"MSH|^~\&|CRA||||20171030215008||ORU^R01|99900000001|P|2.3||||AL
PID|1||PATIENTID||TestSuperShortBreast^Stephanie||19700923|F
PV1|1|O|||||123^Walker^Jane||||||||||||99912345
OBR|1||99923456|100^Risk Assessment^CRA|||20170808080023|20170808080023||||||||123^Walker^Jane|||||||||F
OBX|2|TX|110^Screening Mammography Patient Letter^CRA||Hello~Test~||||||F
OBX|20|NM|220^Lifetime Breast Cancer Risk Score^CRA|Gail|29.01|%^Percent^ISO|||||F
OBX|29|CE|400^Lifetime Breast Cancer Risk Category^CRA||HIGH||||||F
OBX|32|CE|500^Family History^CRA||SELF^self^F^51^&LCIS^~NMTH^mother^F^^&Breast Cancer^45~NFTH^father^M^^&Colon or Rectal Cancer^40~PGRFTH^paternal grandfather^M^^&Colon or Rectal Cancer^45~MCOUSN^maternal cousin^F^^&Breast Cancer^44~PAUNT^paternal aunt^F^^&Uterine Cancer^42~PUNCLE^paternal uncle^M^^&Brain Cancer||||||F
OBX|34|TX|1000.consentYesNo^Do you agree to participate in this survey?||Yes||||||F
OBX|35|TX|1000.surveyQuestion1^Survey Question 1?||Survey Answer 1||||||F
OBX|36|TX|1000.surveyQuestion2^Survey Question 2?||Survey Answer 2||||||F
OBX|37|TX|1000.surveyQuestion3^Survey Question 3?||Survey Answer 3||||||F";

            string errorString = String.Format(HL7Resources.RISK_LETTER_NOT_CREATED, "PATIENTID");
            _logger.Stub(x => x.Log(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning, errorString, messageBody: message)).Return("Error");
            _participantService.Stub(x => x.DoesHashedNHSNumberExist("PATIENTID")).Return(true);
            QuestionnaireResponse response = new QuestionnaireResponse() { };
            QuestionnaireResponse resp2;
            _responseService.Stub(x => x.CreateQuestionnaireHeader("PATIENTID", "20170808080023", "20170808080023", out resp2)).OutRef(response).Return(true);

            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode == "NFTH"))).Return(false);
            _responseService.Stub(x => x.CreateFamilyHistoryItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<FamilyHistoryItem>.Matches(y => y.RelationshipCode != "NFTH"))).Return(true);

            _participantService.Stub(x => x.SetConsentFlag("PATIENTID")).Return(true);

            _responseService.Stub(x => x.CreateResponseItem(Arg<QuestionnaireResponse>.Matches(z => z.Id == 0), Arg<string>.Is.Equal("surveyQuestion1"), Arg<string>.Is.Equal("Survey Answer 1"))).Return(true);

            _participantService.Stub(x => x.CreateRiskLetter("PATIENTID", "29.01", "HIGH", new List<string>() { "Hello", "Test", "" })).Return(true);


            CRAService service = CreateService();
            List<string> ret = service.ProcessQuestionnaire(message);

            Assert.AreEqual(0, ret.Count);
            

        }
    }
}
