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



        protected void SetUpMocks()
        {
            _unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

            _contextService = MockRepository.GenerateMock<IContextService>();
            _configService = MockRepository.GenerateMock<IConfigService>();
            _logger = MockRepository.GenerateMock<IWebJobLogger>();
            _participantService = MockRepository.GenerateMock<IWebJobParticipantService>();
            _responseService = MockRepository.GenerateMock<IResponseService>();
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
        }
    }
}
