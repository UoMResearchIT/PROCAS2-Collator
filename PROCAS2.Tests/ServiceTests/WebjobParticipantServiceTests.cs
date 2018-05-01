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
    public class WebJobParticipantServiceTests:BaseServiceTest
    {
        [SetUp]
        public void SetUp()
        {
            SetUpMocks();
            SetUpStubs();

            FillParticipantRepo();
            FillAppUserRepo();
            FillEventTypeRepo();
        }

        [TearDown]
        public void TearDown()
        {
            ClearMocks();
        }

        private WebJobParticipantService CreateService()
        {
            return new WebJobParticipantService(_participantRepo, _eventRepo, _eventTypeRepo, _unitOfWork, _appUserRepo, _riskLetterRepo);
        }

        [Test]
        public void DoesHashedNHSNumberExist_If_Exists_Returns_True()
        {
            WebJobParticipantService service = CreateService();

            bool ret = service.DoesHashedNHSNumberExist("HashNHS1234");

            Assert.AreEqual(true, ret);
        }

        [Test]
        public void DoesHashedNHSNumberExist_If_Not_Exists_Returns_False()
        {
            WebJobParticipantService service = CreateService();

            bool ret = service.DoesHashedNHSNumberExist("Invalid!");

            Assert.AreEqual(false, ret);
        }

        [Test]
        public void GetSystemUser_If_Not_Exists_Returns_Null()
        {
            WebJobParticipantService service = CreateService();

            AppUser ret = service.GetSystemUser("Invalid!");

            Assert.AreEqual(null, ret);
        }

        [Test]
        public void GetSystemUser_If_Exists_And_System_And_Active_Returns_Correct_User()
        {
            WebJobParticipantService service = CreateService();

            AppUser ret = service.GetSystemUser("CRA");

            Assert.AreEqual("CRA", ret.UserCode);
        }

        [Test]
        public void GetSystemUser_If_Exists_And_Not_System_And_Active_Returns_Null()
        {
            WebJobParticipantService service = CreateService();

            AppUser ret = service.GetSystemUser("andrew");

            Assert.AreEqual(null, ret);
        }

        [Test]
        public void GetSystemUser_If_Exists_And_System_And_Not_Active_Returns_Null()
        {
            WebJobParticipantService service = CreateService();

            AppUser ret = service.GetSystemUser("InactiveCRA");

            Assert.AreEqual(null, ret);
        }

        [Test]
        public void SetConsented_If_Exists_Returns_True()
        {
            WebJobParticipantService service = CreateService();

            bool ret = service.SetConsentFlag("HashNHS1234");

            Assert.AreEqual(true, ret);
            Assert.AreEqual(true, participantList.Where(x => x.HashedNHSNumber=="HashNHS1234").FirstOrDefault().Consented);
        }

        [Test]
        public void SetConsented_If_Not_Exists_Returns_False()
        {
            WebJobParticipantService service = CreateService();

            bool ret = service.SetConsentFlag("Invalid!");

            Assert.AreEqual(false, ret);
        }


        [Test]
        public void CreateRiskLetter_If_Exists_Returns_True()
        {
            WebJobParticipantService service = CreateService();

            bool ret = service.CreateRiskLetter("HashNHS1234", "20", "HIGH", new List<string>());

            Assert.AreEqual(true, ret);
            
        }

        [Test]
        public void CreateRiskLetter_If_Not_Exists_Returns_False()
        {
            WebJobParticipantService service = CreateService();

            bool ret = service.CreateRiskLetter("Invalid!", "20", "HIGH", new List<string>());

            Assert.AreEqual(false, ret);
        }


    }
}
