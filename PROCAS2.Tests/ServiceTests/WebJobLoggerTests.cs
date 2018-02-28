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
using PROCAS2.Services.App;

namespace PROCAS2.Tests.ServiceTests
{
    public class WebJobLoggerTests:BaseServiceTest
    {
        [SetUp]
        public void SetUp()
        {
            SetUpMocks();
            SetUpStubs();

            FillWebJobLogRepo();

        }

        [TearDown]
        public void TearDown()
        {
            ClearMocks();
        }

        private WebJobLogger CreateService()
        {
            return new WebJobLogger(_unitOfWork, _logRepo, _configService);
        }

        [Test]
        public void Log_Message_Over_Min_Level_Returns_Message()
        {
            WebJobLogger service = CreateService();
            _configService.Stub(x => x.GetAppSetting("CRAMinLogLevel")).Return("2");

            string ret = service.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Error, "message");

            Assert.AreEqual("message", ret);

        }

        [Test]
        public void Log_Message_Equal_Min_Level_Returns_Message()
        {
            WebJobLogger service = CreateService();
            _configService.Stub(x => x.GetAppSetting("CRAMinLogLevel")).Return("2");

            string ret = service.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning, "message");

            Assert.AreEqual("message", ret);

        }

        [Test]
        public void Log_Message_Under_Min_Level_Returns_Null()
        {
            WebJobLogger service = CreateService();
            _configService.Stub(x => x.GetAppSetting("CRAMinLogLevel")).Return("2");

            string ret = service.Log(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Info, "message");

            Assert.AreEqual(null, ret);

        }


        [Test]
        public void GetLogCount_Returns_Only_Unreviewed_MessageType_GTE_Log_Level()
        {
            WebJobLogger service = CreateService();

            int count = service.GetLogCount(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Info);

            Assert.AreEqual(webJobLogList.Where(x => x.MessageType == (int)WebJobLogMessageType.CRA_Consent && x.Reviewed == false && x.LogLevel >= (int)WebJobLogLevel.Info).Count(), count);

            int count2 = service.GetLogCount(WebJobLogMessageType.CRA_Consent, WebJobLogLevel.Warning);

            Assert.AreEqual(webJobLogList.Where(x => x.MessageType == (int)WebJobLogMessageType.CRA_Consent && x.Reviewed == false && x.LogLevel >= (int)WebJobLogLevel.Warning).Count(), count2);

            int count3 = service.GetLogCount(WebJobLogMessageType.CRA_Survey, WebJobLogLevel.Warning);

            Assert.AreEqual(webJobLogList.Where(x => x.MessageType == (int)WebJobLogMessageType.CRA_Survey && x.Reviewed == false && x.LogLevel >= (int)WebJobLogLevel.Warning).Count(), count3);

        }

        [Test]
        public void GetAllCurrentErrors_Return_Correct_Number()
        {
            WebJobLogger service = CreateService();

            List<WebJobLog> ret = service.GetAllCurrentErrors();

            Assert.AreNotEqual(ret, null);
            Assert.AreEqual(webJobLogList.Where(x => x.Reviewed == false).Count(), ret.Count());
        }

        [Test]
        public void Review_Invalid_Id_Returns_False()
        {
            WebJobLogger service = CreateService();

            bool ret = service.Review(999);

            Assert.AreEqual(false, ret);
        }


        [Test]
        public void Review_Valid_Id_Returns_True()
        {
            WebJobLogger service = CreateService();

            bool ret = service.Review(3);

            Assert.AreEqual(true, ret);
            Assert.AreEqual(true, webJobLogList.Where(x => x.Id == 3).FirstOrDefault().Reviewed);
        }
    }
}
