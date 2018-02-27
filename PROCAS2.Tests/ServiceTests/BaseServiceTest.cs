using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Mocks;

using PROCAS2.Services.Utility;
using PROCAS2.Services.App;
using PROCAS2.Data;
using PROCAS2.Data.Entities;

namespace PROCAS2.Tests.ServiceTests
{
    public abstract class BaseServiceTest
    {
        protected IUnitOfWork _unitOfWork;
       

        protected IContextService _contextService;
        protected IWebJobLogger _logger;
        protected IWebJobParticipantService _participantService;
        protected IResponseService _responseService;
        protected IConfigService _configService;

        protected IGenericRepository<Participant> _participantRepo;
        protected IGenericRepository<AppUser> _appUserRepo;
        protected IGenericRepository<ParticipantEvent> _eventRepo;
        protected IGenericRepository<EventType> _eventTypeRepo;
        protected IGenericRepository<RiskLetter> _riskLetterRepo;

        protected void SetUpMocks()
        {
            _unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

            _contextService = MockRepository.GenerateMock<IContextService>();
            _configService = MockRepository.GenerateMock<IConfigService>();
            _logger = MockRepository.GenerateMock<IWebJobLogger>();
            _participantService = MockRepository.GenerateMock<IWebJobParticipantService>();
            _responseService = MockRepository.GenerateMock<IResponseService>();


            _participantRepo = MockRepository.GenerateMock<IGenericRepository<Participant>>();
            _appUserRepo = MockRepository.GenerateMock<IGenericRepository<AppUser>>();
            _eventRepo = MockRepository.GenerateMock<IGenericRepository<ParticipantEvent>>();
            _eventTypeRepo = MockRepository.GenerateMock<IGenericRepository<EventType>>();
            _riskLetterRepo = MockRepository.GenerateMock<IGenericRepository<RiskLetter>>();
        }

        protected void SetUpStubs()
        {
            _configService.Stub(x => x.GetAppSetting("HL7RiskLetterCode")).Return("110");
            _configService.Stub(x => x.GetAppSetting("HL7RiskScoreCode")).Return("220");
            _configService.Stub(x => x.GetAppSetting("HL7RiskCategoryCode")).Return("400");
            _configService.Stub(x => x.GetAppSetting("HL7SurveyQuestionCode")).Return("1000");
            _configService.Stub(x => x.GetAppSetting("HL7ConsentCode")).Return("1000.consentYesNo");
            _configService.Stub(x => x.GetAppSetting("HL7FamilyHistoryCode")).Return("500");

           
        }

        protected List<Participant> participantList;
        protected void FillParticipantRepo()
        {
            participantList = new List<Participant>()
            {
                new Participant() { Id = 1, FirstName="Andrew", LastName = "Jerrison", NHSNumber="NHS1234", HashedNHSNumber="HashNHS1234" },
                new Participant() { Id = 2, FirstName="Namby", LastName = "Pamby", NHSNumber="NHS1235", HashedNHSNumber="HashNHS1235" }
            };

            _participantRepo.Stub(x => x.GetAll()).Return(participantList.AsQueryable());
        }

        protected List<AppUser> appUserList;
        protected void FillAppUserRepo()
        {
            appUserList = new List<AppUser>()
            {
                new AppUser() { Id = 1, UserCode="andrew", Active=true, SuperUser= true, SystemUser= false },
                new AppUser() { Id = 2, UserCode="CRA", Active=true, SuperUser = false, SystemUser = true },
                new AppUser() { Id = 3, UserCode="InactiveCRA", Active=false, SuperUser = false, SystemUser = true }

            };

            _appUserRepo.Stub(x => x.GetAll()).Return(appUserList.AsQueryable());
        }


        protected List<EventType> eventTypeList;
        protected void FillEventTypeRepo()
        {
            eventTypeList = new List<EventType>()
            {
                new EventType() { Id = 2, Code="002", Name ="Consent Received" },

                new EventType() { Id = 7, Code="007", Name ="Risk Letter Received" },
            };

            _eventTypeRepo.Stub(x => x.GetAll()).Return(eventTypeList.AsQueryable());
        }

        protected void CompleteSetup()
        {


        }

        protected void ClearMocks()
        {
            _unitOfWork = null;
            _contextService = null;
            _configService = null;
            _logger = null;
            _participantService = null;
            _responseService = null;

            _participantRepo = null;
            _appUserRepo = null;
            _eventRepo = null;
            _eventTypeRepo = null;
            _riskLetterRepo = null;
        }
    }
}
