using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using CsvHelper;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Models.ViewModels;
using PROCAS2.Resources;
using PROCAS2.Services.Utility;

namespace PROCAS2.Services.App
{
    public class ParticipantService:IParticipantService
    {

        private IGenericRepository<Participant> _participantRepo;
        private IGenericRepository<ParticipantEvent> _eventRepo;
        private IGenericRepository<EventType> _eventTypeRepo;
        private IGenericRepository<ScreeningSite> _siteRepo;
        private IGenericRepository<AddressType> _addressTypeRepo;
        private IGenericRepository<Address> _addressRepo;
        private IGenericRepository<RiskLetter> _riskLetterRepo;
        private IGenericRepository<ScreeningRecordV1_5_4> _screeningRepo;
        private IGenericRepository<Image> _imageRepo;
        private IGenericRepository<ParticipantLookup> _lookupRepo;
        private IGenericRepository<VolparaDensity> _densityRepo;
        private IGenericRepository<QuestionnaireResponse> _questionnaireRepo;
        private IGenericRepository<QuestionnaireResponseItem> _questionnaireItemRepo;
        private IGenericRepository<FamilyGeneticTestingItem> _familyGeneticRepo;
        private IGenericRepository<FamilyHistoryItem> _familyHistoryRepo;

        private IUnitOfWork _unitOfWork;
        private IPROCAS2UserManager _userManager;
        private IHashingService _hashingService;
        private IConfigService _configService;
        private IAuditService _auditService;
        private IServiceBusService _serviceBusService;
        private IStorageService _storageService;

        private IHistologyService _histologyService;
       

        private int _UPLOADNEWCOLUMNS;
        private int _UPLOADNEWCOLUMNSEC;
        private int _UPLOADUPDATECOLUMNS;
        private int _UPLOADASKRISKCOLUMNS;
        private int _UPLOADOUTCOMECOLUMNS;
        private DateTime _EARLIESTDOB;
        private DateTime _LATESTDOB;
        private DateTime _EARLIESTDOFA;
        private DateTime _LATESTDOFA;

        public ParticipantService(IUnitOfWork unitOfWork,
                                IGenericRepository<Participant> participantRepo,
                                IGenericRepository<ParticipantEvent> eventRepo,
                                IPROCAS2UserManager userManager,
                                IGenericRepository<EventType> eventTypeRepo,
                                IHashingService hashingService,
                                IConfigService configService,
                                IGenericRepository<ScreeningSite> siteRepo,
                                IGenericRepository<AddressType> addressTypeRepo,
                                IGenericRepository<Address> addressRepo,
                                IGenericRepository<RiskLetter> riskLetterRepo,
                                IGenericRepository<ScreeningRecordV1_5_4> screeningRepo,
                                IGenericRepository<Image> imageRepo,
                                IHistologyService histologyService,
                                IAuditService auditService,
                                IGenericRepository<ParticipantLookup> lookupRepo,
                                IServiceBusService serviceBusService,
                                IStorageService storageService,
                                IGenericRepository<VolparaDensity> densityRepo,
                                IGenericRepository<QuestionnaireResponse> questionnaireRepo,
                                IGenericRepository<QuestionnaireResponseItem> questionnaireItemRepo,
                                IGenericRepository<FamilyGeneticTestingItem> familyGeneticRepo,
                                IGenericRepository<FamilyHistoryItem> familyHistoryRepo)
        {
            _unitOfWork = unitOfWork;
            _participantRepo = participantRepo;
            _eventRepo = eventRepo;
            _userManager = userManager;
            _eventTypeRepo = eventTypeRepo;
            _hashingService = hashingService;
            _configService = configService;
            _siteRepo = siteRepo;
            _addressTypeRepo = addressTypeRepo;
            _addressRepo = addressRepo;
            _riskLetterRepo = riskLetterRepo;
            _screeningRepo = screeningRepo;
            _imageRepo = imageRepo;
            _histologyService = histologyService;
            _auditService = auditService;
            _lookupRepo = lookupRepo;
            _serviceBusService = serviceBusService;
            _storageService = storageService;
            _densityRepo = densityRepo;
            _questionnaireRepo = questionnaireRepo;
            _questionnaireItemRepo = questionnaireItemRepo;
            _familyGeneticRepo = familyGeneticRepo;
            _familyHistoryRepo = familyHistoryRepo;

            // Get the config settings for the uploading. Defaults are deliberately set to be stupid values, to make
            // sure that you set them in the config!
            _UPLOADNEWCOLUMNS = _configService.GetIntAppSetting("UploadNewColumns") ?? 0;
            _UPLOADNEWCOLUMNSEC = _configService.GetIntAppSetting("UploadNewColumnsEC") ?? 0;
            _UPLOADUPDATECOLUMNS = _configService.GetIntAppSetting("UploadUpdateColumns") ?? 0;
            _UPLOADASKRISKCOLUMNS = _configService.GetIntAppSetting("UploadAskRiskColumns") ?? 0;
            _UPLOADOUTCOMECOLUMNS = _configService.GetIntAppSetting("UploadOutcomeColumns") ?? 0;
            _EARLIESTDOB = _configService.GetDateTimeAppSetting("EarliestDOB") ?? DateTime.Now;
            _LATESTDOB = _configService.GetDateTimeAppSetting("LatestDOB") ?? DateTime.Now;
            _EARLIESTDOFA = _configService.GetDateTimeAppSetting("EarliestDOFA") ?? DateTime.Now;
            _LATESTDOFA = _configService.GetDateTimeAppSetting("LatestDOFA") ?? DateTime.Now;
        }

        public bool UploadNewParticipants(UploadNewParticipantsViewModel model, out UploadResultsViewModel outModel, out MemoryStream hashFile)
        {
            return UploadNew(false, model, out  outModel, out hashFile);
        }

        public bool UploadNewParticipantsEC(UploadNewParticipantsViewModel model, out UploadResultsViewModel outModel, out MemoryStream hashFile)
        {
            return UploadNew(true, model, out outModel, out hashFile);
        }

        /// <summary>
        /// Upload a CSV file of new NHS numbers, to create a new placeholder participant record (ready for consent)
        /// </summary>
        /// <param name="EC">true = for East Cheshire, false = not</param>
        /// <param name="model">View model containing the CSV file</param>
        /// <param name="outModel">New view model with any error messages</param>
        /// <param name="hashFile">Memory stream containing the hashes of the NHS numbers.</param>
        /// <returns>true if all the entries in the CSV file are uploaded, else false</returns>
        private bool UploadNew(bool EC, UploadNewParticipantsViewModel model,  out UploadResultsViewModel outModel, out MemoryStream hashFile)
        {
            outModel = new UploadResultsViewModel();

            StreamReader reader = new StreamReader(model.UploadedFile.InputStream);
            int lineCount = 1;
            bool errors = false;
            bool valid = true;

            // First check the file for errors
            while (reader.EndOfStream == false)
            {
                string line = reader.ReadLine();

                valid = ValidateNewParticipantLine(EC, model.Regenerate, line, lineCount, ref outModel);
                if (valid == false)
                {
                    errors = true;
                }
                
                lineCount++;
            }

            hashFile = new MemoryStream();
            // If there are no errors then add them to the database and add the hash to the output spreadsheet
            if (errors == false)
            {
                StringWriter csvString = new StringWriter();
                // First check the file for errors
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                reader.DiscardBufferedData();

                using (var csv = new CsvWriter(csvString))
                {
                    
                    csv.Configuration.Delimiter = "|";

                    
                    while (reader.EndOfStream == false)
                    {
                        string line = reader.ReadLine();
                        string[] lineBits = line.Split(',');

                        string hash = "";
                        int studyNumber;
                        string hashScreen = "";

                        if (model.Regenerate == false)
                        {
                            hash = CreateNewParticipantRecord(EC, lineBits[0], Convert.ToDateTime(lineBits[1]), Convert.ToDateTime(lineBits[2]), EC? lineBits[3]: "", out studyNumber, out hashScreen);
                        }
                        else
                        {
                            hash = _hashingService.CreateNHSHash(lineBits[0]);
                            studyNumber = GetStudyNumber(hash);
                            if (EC)
                            {     
                                hashScreen = _hashingService.CreateScreenHash(lineBits[3]);   
                            }
                        }

                        // First the study number
                        csv.WriteField(studyNumber.ToString().PadLeft(5, '0'));

                        // Then DOB
                        csv.WriteField(Convert.ToDateTime(lineBits[1]).ToString("yyyyMMdd"));

                        // Then date of first appointment
                        csv.WriteField(Convert.ToDateTime(lineBits[2]).ToString("yyyyMMdd"));

                        // Then the hash
                        csv.WriteField(hash);

                        csv.NextRecord();



#if !TESTBUILD // We don't want to start posting messages to the queues if this is just the webnet test version!

                        if (_storageService.StoreInviteMessage(studyNumber.ToString().PadLeft(5, '0'), EC? hashScreen: hash) == false)
                        {
                            // Then the hash
                            csv.WriteField("Error: Invite not sent to Volpara");

                            csv.NextRecord();
                        }

#endif
                    }
                }

                hashFile = new MemoryStream(Encoding.UTF8.GetBytes(csvString.ToString()));

                DeleteOldParticipants(); // Delete any that are 'expired' i.e. invited more than 2 months ago and haven't consented.
                return true;
            }
            else
            { // there were errors
                outModel.DBNoUpdate = true;
                return false;
            }


            
        }


        /// <summary>
        /// Upload the participant's full data
        /// </summary>
        /// <param name="model">The model containing the CSV file</param>
        /// <param name="outModel">The results of the upload (good or bad)</param>
        public void UploadUpdateParticipants(UploadUpdateParticipantsViewModel model, out UploadResultsViewModel outModel)
        {
            outModel = new UploadResultsViewModel();

            StreamReader reader = new StreamReader(model.UploadedFile.InputStream);
            int lineCount = 1;
           
            bool valid = true;

            // First check the file for errors
            while (reader.EndOfStream == false)
            {
                string line = reader.ReadLine();

                valid = ValidateUpdateParticipantLine(line, lineCount, ref outModel);
                if (valid == true)
                {
                    UpdateParticipantRecord(line);
                }

                lineCount++;
            }

         
           
            
        }

        /// <summary>
        /// Upload the screening outcome and update the participant
        /// </summary>
        /// <param name="model">The model containing the CSV file</param>
        /// <param name="outModel">The results of the upload (good or bad)</param>
        public void UploadScreeningOutcomes(UploadScreeningOutcomesViewModel model, out UploadResultsViewModel outModel)
        {
            outModel = new UploadResultsViewModel();
            StreamReader reader = new StreamReader(model.UploadedFile.InputStream);
            int lineCount = 1;

            bool valid = true;

            // First check the file for errors
            while (reader.EndOfStream == false)
            {
                string line = reader.ReadLine();

                // validate the line
                valid = ValidateScreeningOutcomeLine(line, lineCount, ref outModel);
                if (valid == true)
                {
                    string[] lineBits = line.Split(',');
                    string NHSNumber = lineBits[0];
                    // fetch the participant to update
                    Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
                    if (participant != null)
                    {
                        // set the initial outcome
                        string initial = lineBits[1];
                        participant.InitialScreeningOutcome = _lookupRepo.GetAll().Where(x => x.LookupCode == initial).FirstOrDefault();

                        string technical = lineBits[2];
                        string assess = lineBits[3];
                        if (String.IsNullOrEmpty(technical) == false && String.IsNullOrEmpty(assess) == true) // set the final technical outcome
                        {
                            participant.FinalTechnicalOutcome= _lookupRepo.GetAll().Where(x => x.LookupCode == technical).FirstOrDefault();
                            participant.FinalAssessmentOutcome = null;
                        }

                       
                        if (String.IsNullOrEmpty(assess) == false && String.IsNullOrEmpty(technical) == true) // Set the final assessment outcome
                        {
                            participant.FinalAssessmentOutcome = _lookupRepo.GetAll().Where(x => x.LookupCode == assess).FirstOrDefault();
                            participant.FinalTechnicalOutcome = null;
                        }

                        // If the initial outcome is routine then there are no final outcomes
                        if (String.IsNullOrEmpty(technical) && String.IsNullOrEmpty(assess))
                        {
                            participant.FinalTechnicalOutcome = null;
                            participant.FinalAssessmentOutcome = null;
                        }

                        _participantRepo.Update(participant);
                        _unitOfWork.Save();

                        _auditService.AddEvent(participant, _userManager.GetCurrentUser(), DateTime.Now, EventResources.EVENT_SCREENING_OUTCOME, EventResources.EVENT_SCREENING_OUTCOME_STR);



                    }
                }

                lineCount++;
            }
        }


        /// <summary>
        /// Check the the passed line is valid
        /// </summary
        /// <param name="line">The line string</param>
        /// <param name="lineCount">Line number</param>
        /// <param name="outModel">View model to add result messages to</param>
        /// <returns>true if valid, else false</returns>
        private bool ValidateScreeningOutcomeLine(string line, int lineCount, ref UploadResultsViewModel outModel)
        {

            string[] lineBits = line.Split(',');
            if (lineBits.Count() != _UPLOADOUTCOMECOLUMNS) // Should only be 5 columns
            {

                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_INCORRECT_COLUMNS, _UPLOADOUTCOMECOLUMNS), UploadResources.UPLOAD_FAIL);
                return false;
            }

            string NHSNumber = lineBits[0];

            if (lineBits[0].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.NHSNumber)) // NHS numbers are in fact 10 characters long, but DB column was given 12 chars to allow for expansion
            {

                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_TOO_LONG, NHSNumber), UploadResources.UPLOAD_FAIL);
                return false;
            }

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
            if (participant == null) // Participant does not exist in the database
            {

                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_NOT_IN_DB, NHSNumber), UploadResources.UPLOAD_FAIL);
                return false;
            }
            else
            {
                if (participant.Consented == false)
                {
                    outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_NOT_CONSENTED, NHSNumber), UploadResources.UPLOAD_FAIL);
                    return false;
                }
            }

            // initial outcome is mandatory
            if (String.IsNullOrEmpty(lineBits[1]))
            {
                outModel.AddMessage(lineCount, UploadResources.NO_INITIAL_OUTCOME, UploadResources.UPLOAD_FAIL);
                return false;
            }

            // initial code must exist
            string code = lineBits[1];
            ParticipantLookup initial = _lookupRepo.GetAll().Where(x => x.LookupCode == code).FirstOrDefault();
            if (initial == null)
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.INITIAL_OUTCOME_NOT_EXIST, lineBits[1]), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Both types of final outcome cannot be filled in!
            if (String.IsNullOrEmpty(lineBits[2]) == false && String.IsNullOrEmpty(lineBits[3]) == false)
            {
                outModel.AddMessage(lineCount, UploadResources.BOTH_FINAL_OUTCOME_FILLED_IN, UploadResources.UPLOAD_FAIL);
                return false;
            }

            // if the initial outcome is technical then check validity of column 3
            if (lineBits[1] == "INI_TECH")
            {
                // technical outcome is mandatory
                if (String.IsNullOrEmpty(lineBits[2]))
                {
                    outModel.AddMessage(lineCount, UploadResources.NO_TECHNICAL_OUTCOME, UploadResources.UPLOAD_FAIL);
                    return false;
                }

                // technical code must exist
                code = lineBits[2];
                ParticipantLookup technical = _lookupRepo.GetAll().Where(x => x.LookupCode == code).FirstOrDefault();
                if (technical == null)
                {
                    outModel.AddMessage(lineCount, string.Format(UploadResources.TECHNICAL_OUTCOME_NOT_EXIST, lineBits[2]), UploadResources.UPLOAD_FAIL);
                    return false;
                }
            }

            // if the initial outcome is assessment then check validity of column 4
            if (lineBits[1] == "INI_ASSESS")
            {
                // assessment outcome is mandatory
                if (String.IsNullOrEmpty(lineBits[3]))
                {
                    outModel.AddMessage(lineCount, UploadResources.NO_ASSESSMENT_OUTCOME, UploadResources.UPLOAD_FAIL);
                    return false;
                }

                // assessment code must exist
                code = lineBits[3];
                ParticipantLookup assessment = _lookupRepo.GetAll().Where(x => x.LookupCode == code).FirstOrDefault();
                if (assessment == null)
                {
                    outModel.AddMessage(lineCount, string.Format(UploadResources.ASSESSMENT_OUTCOME_NOT_EXIST, lineBits[3]), UploadResources.UPLOAD_FAIL);
                    return false;
                }
            }

            outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_OUTCOME_SUCCESS, NHSNumber), UploadResources.UPLOAD_ASKRISK);
            return true;

        }



        /// <summary>
        /// Update the participant's 'ask for risk lette' flag
        /// </summary>
        /// <param name="model">The model containing the CSV file</param>
        /// <param name="outModel">The results of the upload (good or bad)</param>
        public void UploadAskRisk(UploadAskRiskViewModel model, out UploadResultsViewModel outModel)
        {
            outModel = new UploadResultsViewModel();

            StreamReader reader = new StreamReader(model.UploadedFile.InputStream);
            int lineCount = 1;

            bool valid = true;

            // First check the file for errors
            while (reader.EndOfStream == false)
            {
                string line = reader.ReadLine();

                valid = ValidateAskRiskLine(line, lineCount, ref outModel);
                if (valid == true)
                {
                    string[] lineBits = line.Split(',');
                    string NHSNumber = lineBits[0];
                    Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
                    if (participant != null)
                    {
                        participant.AskForRiskLetter = true;
                        _participantRepo.Update(participant);
                        _unitOfWork.Save();

                        _auditService.AddEvent(participant, _userManager.GetCurrentUser(), DateTime.Now, EventResources.EVENT_ASK_RISK, EventResources.EVENT_ASK_RISK_STR);
                        
                        

                    }
                }

                lineCount++;
            }




        }

        /// <summary>
        /// Check the the passed line is valid
        /// </summary
        /// <param name="line">The line string</param>
        /// <param name="lineCount">Line number</param>
        /// <param name="outModel">View model to add result messages to</param>
        /// <returns>true if valid, else false</returns>
        private bool ValidateAskRiskLine(string line, int lineCount, ref UploadResultsViewModel outModel)
        {

            string[] lineBits = line.Split(',');
            if (lineBits.Count() != _UPLOADASKRISKCOLUMNS) // Should only be 2 columns
            {

                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_INCORRECT_COLUMNS, _UPLOADASKRISKCOLUMNS), UploadResources.UPLOAD_FAIL);
                return false;
            }

            string NHSNumber = lineBits[0];

            if (lineBits[0].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.NHSNumber)) // NHS numbers are in fact 10 characters long, but DB column was given 12 chars to allow for expansion
            {

                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_TOO_LONG, NHSNumber), UploadResources.UPLOAD_FAIL);
                return false;
            }

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
            if (participant == null) // Participant does not exist in the database
            {

                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_NOT_IN_DB, NHSNumber), UploadResources.UPLOAD_FAIL);
                return false;
            }
            else
            {
                if (participant.Consented == false)
                {
                    outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_NOT_CONSENTED, NHSNumber), UploadResources.UPLOAD_FAIL);
                    return false;
                }
            }

            // <ust say YES!
            if (lineBits[1].ToUpper() != "YES")
            {
                outModel.AddMessage(lineCount, UploadResources.UPLOAD_MUST_BE_YES, UploadResources.UPLOAD_FAIL);
                return false;
            }

            outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ASK_RISK_SUCCESS, NHSNumber), UploadResources.UPLOAD_ASKRISK);
            return true;

        }


        /// <summary>
        /// Get the study number for the passed hashed NHS number
        /// </summary>
        /// <param name="hash">Hashed NHS number</param>
        /// <returns>study number</returns>
        public int GetStudyNumber(string hash)
        {
            Participant participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hash).FirstOrDefault();
            if (participant != null)
            {
                return participant.StudyNumber;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Create a new participant record
        /// </summary>
        /// <param name="EC">true = for East Cheshire</param>
        /// <param name="NHSNumber">NHS number</param>
        /// <param name="DOB">Date of birth</param>
        /// <param name="DOFA">Date of first appointment</param>
        /// <param name="screeningNumber">Screening number (only for East Cheshire)</param>
        /// <param name="studyNumber">study number returned</param>
        /// <returns>Hashed NHS number</returns>
        private string CreateNewParticipantRecord(bool EC, string NHSNumber, DateTime DOB, DateTime DOFA, string screeningNumber, out int studyNumber, out string hashScreen)
        {
            DateTime dateCreated = DateTime.Now;

            Participant participant = new Participant();
            participant.NHSNumber = NHSNumber;
            string hash = _hashingService.CreateNHSHash(NHSNumber);
            participant.HashedNHSNumber = hash;
            hashScreen = "";
            if (EC) {
                hashScreen = _hashingService.CreateScreenHash(screeningNumber);
                participant.HashedScreeningNumber = hashScreen;
                participant.UseScreeningNumber = true;
            }
            else
            {
                participant.UseScreeningNumber = false;
            }

            participant.DateCreated = dateCreated;
            participant.DateOfBirth = DOB;
            participant.DateFirstAppointment = DOFA;
            studyNumber = CreateNextStudyNumber();
            participant.StudyNumber = studyNumber;
            _participantRepo.Insert(participant);

            _unitOfWork.Save();

            _auditService.AddEvent(participant, _userManager.GetCurrentUser(), dateCreated, EventResources.EVENT_CREATED, EventResources.EVENT_CREATED_STR);


            return hash;
        }

        /// <summary>
        /// Check the the passed line is valid
        /// </summary
        /// <param name="EC">true = for East Cheshire</param>
        /// <param name="regenerate">True = regenerating the hash file, false = initial upload</param>
        /// <param name="line">The line string</param>
        /// <param name="lineCount">Line number</param>
        /// <param name="outModel">View model to add result messages to</param>
        /// <returns>true if valid, else false</returns>
        private bool ValidateNewParticipantLine(bool EC, bool regenerate, string line, int lineCount, ref UploadResultsViewModel outModel)
        {
            
            string[] lineBits = line.Split(',');

            if (EC)
            {
                if (lineBits.Count() != _UPLOADNEWCOLUMNSEC) // Should only be 4 columns
                {
                    outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_INCORRECT_COLUMNS, 4), UploadResources.UPLOAD_FAIL);
                    return false;
                }
            }
            else
            {
                if (lineBits.Count() != _UPLOADNEWCOLUMNS) // Should only be 3 columns
                {
                    outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_INCORRECT_COLUMNS, 3), UploadResources.UPLOAD_FAIL);
                    return false;
                }
            }

            string NHSNumber = lineBits[0];

            if (lineBits[0].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.NHSNumber)) // NHS numbers are in fact 10 characters long, but DB column was given 20 chars to allow for expansion and testing
            {
               
                outModel.AddMessage(lineCount , string.Format(UploadResources.UPLOAD_NHS_NUMBER_TOO_LONG, NHSNumber), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Date of Birth
            if (String.IsNullOrEmpty(lineBits[1]) == true) // date of birth is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOB_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            DateTime DOB;
            if (DateTime.TryParse(lineBits[1], out DOB) == false) // check out the format
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOB_WRONG_FORMAT), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (DOB < _EARLIESTDOB || DOB > _LATESTDOB)
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOB_OUT_OF_RANGE), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Date of first appointment
            if (String.IsNullOrEmpty(lineBits[2]) == true) // date of first appointment is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOFA_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            DateTime DOFA;
            if (DateTime.TryParse(lineBits[2], out DOFA) == false) // check out the format
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOFA_WRONG_FORMAT), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (DOFA < _EARLIESTDOFA || DOFA > _LATESTDOFA)
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOFA_OUT_OF_RANGE), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Screening number (required for East Cheshire)
            if (EC && String.IsNullOrEmpty(lineBits[3]) == true) // screening number is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_SCREENING_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
            if (participant != null) // Participant already exists in the database
            {

                if (regenerate == false) // Only care about this error if it is the initial upload, not when regenerating the hash file
                {
                    outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_IN_DB, NHSNumber), UploadResources.UPLOAD_FAIL);
                    return false;
                }
            }
            else
            {
                if (regenerate == true) // Patient not in DB so don't create a hash if on regenerate mode.
                {
                    outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_NOT_IN_DB, NHSNumber), UploadResources.UPLOAD_FAIL);
                    return false;
                }
            }

            
            return true;

        }


        /// <summary>
        /// Create a new participant record
        /// Note: It is assumed that most of the validation has happened beforehand!
        /// </summary>
        /// <param name="line">line from CSV file</param>
        private void UpdateParticipantRecord(string line)
        {
            string[] lineBits = line.Split(',');
            if (lineBits.Count() == _UPLOADUPDATECOLUMNS) // Should be 21 columns
            {
                string NHSNumber = lineBits[0];
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
                if (participant != null)
                {
                    DateTime DOFA = Convert.ToDateTime(lineBits[3]);
                    DateTime DOB = Convert.ToDateTime(lineBits[2]);

                    participant.ScreeningNumber = lineBits[1];
                    participant.DateFirstAppointment = DOFA;
                    participant.DateActualAppointment = DOFA; // Initially the 'actual' appointment date will be the same as the first
                    participant.DateOfBirth = DOB;
                    participant.Title = String.IsNullOrEmpty(lineBits[4])? null: lineBits[4];
                    participant.FirstName = lineBits[5];
                    participant.LastName = lineBits[6];
                    
                    participant.GPName = lineBits[12];

                    string siteCode = lineBits[19];
                    ScreeningSite site = _siteRepo.GetAll().Where(x => x.Code == siteCode).FirstOrDefault();
                    if (site != null)
                    {
                        participant.ScreeningSite = site;
                    }

                    _participantRepo.Update(participant);
                    _unitOfWork.Save();

                    Address homeAddress = new Address();
                    homeAddress.AddressLine1 = lineBits[7];
                    homeAddress.AddressLine2 = String.IsNullOrEmpty(lineBits[8]) ? null : lineBits[8]; ;
                    homeAddress.AddressLine3 = String.IsNullOrEmpty(lineBits[9]) ? null : lineBits[9]; ;
                    homeAddress.AddressLine4 = String.IsNullOrEmpty(lineBits[10]) ? null : lineBits[10]; ;
                    homeAddress.PostCode = lineBits[11];
                    
                    homeAddress.Participant = participant;
                    homeAddress.AddressType = _addressTypeRepo.GetAll().Where(x => x.Name == "HOME").FirstOrDefault();

                    _addressRepo.Insert(homeAddress);
                    _unitOfWork.Save();

                    Address gpAddress = new Address();
                    gpAddress.AddressLine1 = lineBits[13];
                    gpAddress.AddressLine2 = String.IsNullOrEmpty(lineBits[14]) ? null : lineBits[14]; ;
                    gpAddress.AddressLine3 = String.IsNullOrEmpty(lineBits[15]) ? null : lineBits[15]; ;
                    gpAddress.AddressLine4 = String.IsNullOrEmpty(lineBits[16]) ? null : lineBits[16]; ;
                    gpAddress.PostCode = lineBits[17];
                    gpAddress.EmailAddress = String.IsNullOrEmpty(lineBits[18]) ? null : lineBits[18]; ;
                    gpAddress.Participant = participant;
                    gpAddress.AddressType = _addressTypeRepo.GetAll().Where(x => x.Name == "GP").FirstOrDefault();

                    _addressRepo.Insert(gpAddress);
                    _unitOfWork.Save();

                    _auditService.AddEvent(participant, _userManager.GetCurrentUser(), DateTime.Now, EventResources.EVENT_UPDATED, EventResources.EVENT_UPDATED_STR);

                }
            }

            
        }

        /// <summary>
        /// Check the the passed line is valid
        /// </summary
        /// <param name="line">The line string</param>
        /// <param name="lineCount">Line number</param>
        /// <param name="outModel">View model to add result messages to</param>
        /// <returns>true if valid, else false</returns>
        private bool ValidateUpdateParticipantLine(string line, int lineCount, ref UploadResultsViewModel outModel)
        {

            string[] lineBits = line.Split(',');
            if (lineBits.Count() != _UPLOADUPDATECOLUMNS) // Should be 23 columns
            {

                outModel.AddMessage( lineCount , string.Format(UploadResources.UPLOAD_INCORRECT_COLUMNS, 23), UploadResources.UPLOAD_FAIL);
                return false;
            }

            string NHSNumber = lineBits[0];

            if (String.IsNullOrEmpty(lineBits[0]) == true) // NHS number is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[0].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.NHSNumber)) // NHS numbers are in fact 10 characters long, but DB column was given 12 chars to allow for expansion
            {

                outModel.AddMessage(lineCount , string.Format(UploadResources.UPLOAD_NHS_NUMBER_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
            if (participant == null) // Participant does not exist in the database
            {

                outModel.AddMessage(lineCount ,string.Format(UploadResources.UPLOAD_NHS_NUMBER_NOT_IN_DB, NHSNumber), UploadResources.UPLOAD_FAIL);
                return false;
            }
            else
            {
                if (participant.Consented == false)
                {
                    outModel.AddMessage( lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_NOT_CONSENTED, NHSNumber), UploadResources.UPLOAD_FAIL);
                    return false;
                }

                if (String.IsNullOrEmpty(participant.FirstName) == false)
                {
                    outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_NHS_NUMBER_ALREADY_UPDATED, NHSNumber), UploadResources.UPLOAD_FAIL);
                    return false;
                }
            }

            // Screening number
            if (String.IsNullOrEmpty(lineBits[1]) == true) // Screening number is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_SCREEN_NUMBER_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[1].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.ScreeningNumber)) // should be 20 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_SCREEN_NUMBER_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Date of Birth
            if (String.IsNullOrEmpty(lineBits[2]) == true) // date of birth is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOB_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            DateTime DOB;
            if (DateTime.TryParse(lineBits[2], out DOB) == false) // check out the format
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOB_WRONG_FORMAT), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (DOB < _EARLIESTDOB || DOB > _LATESTDOB)
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOB_OUT_OF_RANGE), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Date of first appointment
            if (String.IsNullOrEmpty(lineBits[3]) == true) // date of first appointment is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOFA_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            DateTime DOFA;
            if (DateTime.TryParse(lineBits[3], out DOFA) == false) // check out the format
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOFA_WRONG_FORMAT), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (DOFA < _EARLIESTDOFA || DOFA > _LATESTDOFA)
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_DOFA_OUT_OF_RANGE), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Title
            if (lineBits[4].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.Title)) // should be 50 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_TITLE_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // First name
            if (String.IsNullOrEmpty(lineBits[5]) == true) // First name is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_FIRST_NAME_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[5].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.FirstName)) // should be 50 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_FIRST_NAME_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Last name
            if (String.IsNullOrEmpty(lineBits[6]) == true) // Last name is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_LAST_NAME_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[6].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.LastName)) // should be 50 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_LAST_NAME_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

           
            // Address line 1
            if (String.IsNullOrEmpty(lineBits[7]) == true) // First address line is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_1_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[7].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine1)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_1_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }


            // Address line 2
            if (lineBits[8].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine2)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_2_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Address line 3
            if (lineBits[9].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine3)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_3_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Address line 4
            if (lineBits[10].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine4)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_4_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Postcode
            if (String.IsNullOrEmpty(lineBits[11]) == true) // Postcode is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_POSTCODE_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[11].Length > AttributeHelpers.GetMaxLength<Address>(x => x.PostCode)) // should be 10 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_POSTCODE_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

          

            // GP Name
            if (String.IsNullOrEmpty(lineBits[12]) == true) // GP Name is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_NAME_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[12].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.GPName)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_NAME_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }


            // GP Address line 1
            if (String.IsNullOrEmpty(lineBits[13]) == true) // First GP address line is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_1_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[13].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine1)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_1_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }


            // GP Address line 2
            if (lineBits[14].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine2)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_2_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // GP Address line 3
            if (lineBits[15].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine3)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_3_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // GP Address line 4
            if (lineBits[16].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine4)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_4_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // GP Postcode
            if (String.IsNullOrEmpty(lineBits[17]) == true) // GP Postcode is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_POSTCODE_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[17].Length > AttributeHelpers.GetMaxLength<Address>(x => x.PostCode)) // should be 10 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_POSTCODE_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // GP Email address
            if (lineBits[18].Length > AttributeHelpers.GetMaxLength<Address>(x => x.EmailAddress)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_EMAIL_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Site code
            if (String.IsNullOrEmpty(lineBits[19]) == true) // Site code is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_SITE_CODE_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[19].Length > AttributeHelpers.GetMaxLength<ScreeningSite>(x => x.Code)) // should be 10 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_SITE_CODE_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            string siteCode = lineBits[19];
            ScreeningSite site = _siteRepo.GetAll().Where(x => x.Code == siteCode ).FirstOrDefault();
            if (site == null)
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_SITE_CODE_WRONG, siteCode), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // We've managed to pass all the checks so it must be valid!
            outModel.AddMessage(lineCount , string.Format(UploadResources.UPLOAD_NHS_NUMBER_SUCCESS, NHSNumber), UploadResources.UPLOAD_UPDATED);
            return true;

        }

        /// <summary>
        /// Validate the view model prior to saving
        /// </summary>
        /// <param name="model">View model</param>
        /// <returns>List of errors</returns>
        private List<string> ValidateParticipantViewModel(ParticipantEditViewModel model)
        {
            List<string> errors = new List<string>();

            // Date of birth
            if (model.DOB < _EARLIESTDOB || model.DOB > _LATESTDOB)
            {
                errors.Add(UploadResources.UPLOAD_DOB_OUT_OF_RANGE);
            }

            // First appointment
            if (model.DOFA < _EARLIESTDOFA || model.DOFA > _LATESTDOFA)
            {
                errors.Add(UploadResources.UPLOAD_DOFA_OUT_OF_RANGE);
            }

            // Actual appointment
            if (model.DOAA < _EARLIESTDOFA || model.DOAA > _LATESTDOFA) // Range should be the same as the first appointment!
            {
                errors.Add(UploadResources.UPLOAD_DOAA_OUT_OF_RANGE);
            }

            return errors;
        }

        /// <summary>
        /// Update the participant record using the view model from the Edit screen
        /// </summary>
        /// <param name="model">View model containing the information</param>
        /// <returns>List of errors</returns>
        public List<string> UpdateParticipantFromUI(ParticipantEditViewModel model)
        {
            List<string> errors = new List<string>();

            errors = ValidateParticipantViewModel(model);

            if (errors.Count > 0)
            {
                return errors;
            }

            try
            {
                // Double check the participant exists!
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == model.NHSNumber).FirstOrDefault();
                if (participant != null)
                {
                    if (!String.IsNullOrEmpty(model.BMI))
                    {
                        participant.BMI = _auditService.ChangeEventDouble(participant, ParticipantResources.BMI, participant.BMI, Convert.ToDouble(model.BMI), model.Reason);
                    }
                    else
                    {
                        participant.BMI = _auditService.ChangeEventDouble(participant, ParticipantResources.BMI, participant.BMI, null, model.Reason);

                    }
                    participant.Chemoprevention = _auditService.ChangeEventBool(participant, ParticipantResources.CHEMO , participant.Chemoprevention, model.Chemo, model.Reason);
                    participant.ChemoAgreedInClinic = _auditService.ChangeEventBool(participant, ParticipantResources.CHEMO_AGREED, participant.ChemoAgreedInClinic, model.ChemoAgreedInClinic, model.Reason);

                    participant.Consented = _auditService.ChangeEventBool(participant, ParticipantResources.CONSENTED, participant.Consented, model.Consented, model.Reason);
                    participant.DateActualAppointment = _auditService.ChangeEventDate(participant, ParticipantResources.DOAA, participant.DateActualAppointment, model.DOAA, model.Reason);
                    participant.DateFirstAppointment = _auditService.ChangeEventDate(participant, ParticipantResources.DOFA, participant.DateFirstAppointment, model.DOFA, model.Reason);
                    participant.DateOfBirth = _auditService.ChangeEventDate(participant, ParticipantResources.DOB, participant.DateOfBirth, model.DOB, model.Reason);
                    participant.Deceased = _auditService.ChangeEventBool(participant, ParticipantResources.DECEASED, participant.Deceased, model.Deceased, model.Reason);
                    participant.Diagnosed = _auditService.ChangeEventBool(participant, ParticipantResources.DIAGNOSED, participant.Diagnosed, model.Diagnosed, model.Reason);
                    participant.FHCReferral = _auditService.ChangeEventBool(participant, ParticipantResources.FHC_REFERRAL, participant.FHCReferral, model.FHCReferral, model.Reason);
                    participant.MoreFrequentScreening = _auditService.ChangeEventBool(participant, ParticipantResources.MORE_FREQUENT, participant.MoreFrequentScreening, model.MoreFrequentScreening, model.Reason);
                    participant.FirstName = _auditService.ChangeEventString(participant, ParticipantResources.FIRST_NAME, participant.FirstName, model.FirstName, model.Reason);
                    participant.GPName = _auditService.ChangeEventString(participant, ParticipantResources.GP_NAME, participant.GPName, model.GPName, model.Reason);
                    participant.LastName = _auditService.ChangeEventString(participant, ParticipantResources.LAST_NAME, participant.LastName, model.LastName, model.Reason);
                    participant.ScreeningNumber = _auditService.ChangeEventString(participant, ParticipantResources.SCREENING_NUMBER, participant.ScreeningNumber, model.ScreeningNumber, model.Reason);

                    ScreeningSite newSite = _siteRepo.GetAll().Where(x => x.Code == model.ScreeningSite).FirstOrDefault();
                    _auditService.ChangeEventString(participant, ParticipantResources.SCREENING_SITE, participant.ScreeningSite.Code, newSite.Code, model.Reason);
                    participant.ScreeningSite = newSite;
                    
                    participant.SentRisk = _auditService.ChangeEventBool(participant, ParticipantResources.SENT_RISK, participant.SentRisk, model.SentRisk, model.Reason);
                    participant.AttendedScreening = _auditService.ChangeEventBool(participant, ParticipantResources.ATTENDED_SCREENING, participant.AttendedScreening, model.AttendedScreening, model.Reason);
                    participant.Title = _auditService.ChangeEventString(participant, ParticipantResources.TITLE, participant.Title, model.Title, model.Reason);
                    participant.Withdrawn = _auditService.ChangeEventBool(participant, ParticipantResources.WITHDRAWN, participant.Withdrawn, model.Withdrawn, model.Reason);
                    participant.MailingList = _auditService.ChangeEventBool(participant, ParticipantResources.MAILING_LIST, participant.MailingList, model.MailingList, model.Reason);
                    participant.AskForRiskLetter = _auditService.ChangeEventBool(participant, ParticipantResources.ASKFORRISK, participant.AskForRiskLetter, model.AskForRiskLetter, model.Reason);

                   
                    participant.DateConsented = _auditService.ChangeEventDate(participant, ParticipantResources.DATE_CONSENTED, participant.DateConsented, model.DateConsented, model.Reason);
                    participant.RiskConsultationBooked = _auditService.ChangeEventBool(participant, ParticipantResources.RISK_CONS_BOOKED, participant.RiskConsultationBooked, model.RiskConsultationBooked, model.Reason);
                    participant.RiskConsultationComments = _auditService.ChangeEventString(participant, ParticipantResources.RISK_CONS_COMMENT, participant.RiskConsultationComments, model.RiskConsultationComments, model.Reason);
                    participant.RiskConsultationEligible = _auditService.ChangeEventBool(participant, ParticipantResources.RISK_CONS_ELIGIBLE, participant.RiskConsultationEligible, model.RiskConsultationEligible, model.Reason);
                    participant.RiskConsultationCompleted = _auditService.ChangeEventBool(participant, ParticipantResources.RISK_CONS_COMPLETED, participant.RiskConsultationCompleted, model.RiskConsultationCompleted, model.Reason);
                    participant.RiskConsultationLetterSent = _auditService.ChangeEventBool(participant, ParticipantResources.RISK_CONS_LETTER_SENT, participant.RiskConsultationLetterSent, model.RiskConsultationLetterSent, model.Reason);


                    if (participant.RiskConsultationType != null && model.RiskConsultationTypeId == null)
                    {
                        _auditService.ChangeEventString(participant, ParticipantResources.RISK_CONS_TYPE, participant.RiskConsultationType.LookupDescription, "", model.Reason);
                        participant.RiskConsultationType = null;
                    }
                    else if (participant.RiskConsultationType != null && model.RiskConsultationTypeId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.RiskConsultationTypeId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.RISK_CONS_TYPE, participant.RiskConsultationType.LookupDescription, lookup.LookupDescription, model.Reason);
                        participant.RiskConsultationType = lookup;
                    }
                    else if (participant.RiskConsultationType == null && model.RiskConsultationTypeId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.RiskConsultationTypeId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.RISK_CONS_TYPE, "", lookup.LookupDescription, model.Reason);
                        participant.RiskConsultationType = lookup;
                    }

                    if (participant.ChemoPreventionDetails != null && model.ChemoPreventionDetailsId == null)
                    {
                        _auditService.ChangeEventString(participant, ParticipantResources.CHEMO_DETAILS, participant.ChemoPreventionDetails.LookupDescription, "", model.Reason);
                        participant.ChemoPreventionDetails = null;
                    }
                    else if (participant.ChemoPreventionDetails != null && model.ChemoPreventionDetailsId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.ChemoPreventionDetailsId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.CHEMO_DETAILS, participant.ChemoPreventionDetails.LookupDescription, lookup.LookupDescription, model.Reason);
                        participant.ChemoPreventionDetails = lookup;
                    }
                    else if (participant.ChemoPreventionDetails == null && model.ChemoPreventionDetailsId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.ChemoPreventionDetailsId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.CHEMO_DETAILS, "", lookup.LookupDescription, model.Reason);
                        participant.ChemoPreventionDetails = lookup;
                    }

                    if (participant.InitialScreeningOutcome != null && model.InitialScreeningOutcomeId == null)
                    {
                        _auditService.ChangeEventString(participant, ParticipantResources.INITIAL_SCREENING, participant.InitialScreeningOutcome.LookupDescription, "", model.Reason);
                        participant.InitialScreeningOutcome = null;
                    }
                    else if (participant.InitialScreeningOutcome != null && model.InitialScreeningOutcomeId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.InitialScreeningOutcomeId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.INITIAL_SCREENING, participant.InitialScreeningOutcome.LookupDescription, lookup.LookupDescription, model.Reason);
                        participant.InitialScreeningOutcome = lookup;
                    }
                    else if (participant.InitialScreeningOutcome == null && model.InitialScreeningOutcomeId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.InitialScreeningOutcomeId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.INITIAL_SCREENING, "", lookup.LookupDescription, model.Reason);
                        participant.InitialScreeningOutcome = lookup;
                    }

                    if (participant.FinalTechnicalOutcome != null && model.FinalTechnicalOutcomeId == null)
                    {
                        _auditService.ChangeEventString(participant, ParticipantResources.FINAL_TECHNICAL, participant.FinalTechnicalOutcome.LookupDescription, "", model.Reason);
                        participant.FinalTechnicalOutcome = null;
                    }
                    else if (participant.FinalTechnicalOutcome != null && model.FinalTechnicalOutcomeId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.FinalTechnicalOutcomeId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.FINAL_TECHNICAL, participant.FinalTechnicalOutcome.LookupDescription, lookup.LookupDescription, model.Reason);
                        participant.FinalTechnicalOutcome = lookup;
                    }
                    else if (participant.FinalTechnicalOutcome == null && model.FinalTechnicalOutcomeId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.FinalTechnicalOutcomeId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.FINAL_TECHNICAL, "", lookup.LookupDescription, model.Reason);
                        participant.FinalTechnicalOutcome = lookup;
                    }

                    if (participant.FinalAssessmentOutcome != null && model.FinalAssessmentOutcomeId == null)
                    {
                        _auditService.ChangeEventString(participant, ParticipantResources.FINAL_ASSESSMENT, participant.FinalAssessmentOutcome.LookupDescription, "", model.Reason);
                        participant.FinalAssessmentOutcome = null;
                    }
                    else if (participant.FinalAssessmentOutcome != null && model.FinalAssessmentOutcomeId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.FinalAssessmentOutcomeId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.FINAL_ASSESSMENT, participant.FinalAssessmentOutcome.LookupDescription, lookup.LookupDescription, model.Reason);
                        participant.FinalAssessmentOutcome = lookup;
                    }
                    else if (participant.FinalAssessmentOutcome == null && model.FinalAssessmentOutcomeId != null)
                    {
                        ParticipantLookup lookup = _lookupRepo.GetAll().Where(x => x.Id == model.FinalAssessmentOutcomeId).FirstOrDefault();
                        _auditService.ChangeEventString(participant, ParticipantResources.FINAL_ASSESSMENT, "", lookup.LookupDescription, model.Reason);
                        participant.FinalAssessmentOutcome = lookup;
                    }

                    _participantRepo.Update(participant);
                    _unitOfWork.Save();

                    Address homeAdd = _addressRepo.GetAll().Where(x => x.Participant.NHSNumber == model.NHSNumber && x.AddressType.Name == "HOME").FirstOrDefault();
                    if (homeAdd != null)
                    {
                        homeAdd.AddressLine1 = _auditService.ChangeEventString(participant, ParticipantResources.HOME_ADD_1, homeAdd.AddressLine1, model.HomeAddress1, model.Reason);
                        homeAdd.AddressLine2 = _auditService.ChangeEventString(participant, ParticipantResources.HOME_ADD_2, homeAdd.AddressLine2, model.HomeAddress2, model.Reason);
                        homeAdd.AddressLine3 = _auditService.ChangeEventString(participant, ParticipantResources.HOME_ADD_3, homeAdd.AddressLine3, model.HomeAddress3, model.Reason);
                        homeAdd.AddressLine4 = _auditService.ChangeEventString(participant, ParticipantResources.HOME_ADD_4, homeAdd.AddressLine4, model.HomeAddress4, model.Reason);
                        homeAdd.EmailAddress = _auditService.ChangeEventString(participant, ParticipantResources.HOMEEMAIL, homeAdd.EmailAddress, model.HomeEmail, model.Reason);
                        homeAdd.PostCode = _auditService.ChangeEventString(participant, ParticipantResources.HOME_POSTCODE, homeAdd.PostCode, model.HomePostCode, model.Reason);
                        _addressRepo.Update(homeAdd);
                        _unitOfWork.Save();
                    }

                    Address gpAdd = _addressRepo.GetAll().Where(x => x.Participant.NHSNumber == model.NHSNumber && x.AddressType.Name == "GP").FirstOrDefault();
                    if (gpAdd != null)
                    {
                        gpAdd.AddressLine1 = _auditService.ChangeEventString(participant, ParticipantResources.GP_ADD_1, gpAdd.AddressLine1, model.GPAddress1, model.Reason);
                        gpAdd.AddressLine2 = _auditService.ChangeEventString(participant, ParticipantResources.GP_ADD_2, gpAdd.AddressLine2, model.GPAddress2, model.Reason);
                        gpAdd.AddressLine3 = _auditService.ChangeEventString(participant, ParticipantResources.GP_ADD_3, gpAdd.AddressLine3, model.GPAddress3, model.Reason);
                        gpAdd.AddressLine4 = _auditService.ChangeEventString(participant, ParticipantResources.GP_ADD_4, gpAdd.AddressLine4, model.GPAddress4, model.Reason);
                        gpAdd.EmailAddress = _auditService.ChangeEventString(participant, ParticipantResources.GP_EMAIL, gpAdd.EmailAddress, model.GPEmail, model.Reason);
                        gpAdd.PostCode = _auditService.ChangeEventString(participant, ParticipantResources.GP_POSTCODE, gpAdd.PostCode, model.GPPostCode, model.Reason);
                        _addressRepo.Update(gpAdd);
                        _unitOfWork.Save();
                    }

                }
                else // participant does not exist
                {
                    errors.Add(String.Format(ParticipantResources.PARTICIPANT_NOT_FOUND, model.NHSNumber));
                }
            }
            catch(Exception ex)
            {
                errors.Add(ex.Message);
            }

            return errors;
        }

        /// <summary>
        /// Delete anyone who has not consented to join the study within 2 omnths of being invited.
        /// </summary>
        /// <returns>true if the delete has worked, else false</returns>
        public bool DeleteOldParticipants()
        {
            try
            {
                DateTime invitedSince = DateTime.Now.AddMonths(-2);

                List<Participant> participants = _participantRepo.GetAll().Where(x => x.Deleted == false && x.Consented == false && x.DateCreated.HasValue && x.DateCreated < invitedSince).ToList();
                foreach(Participant participant in participants)
                {
                    bool ret = DeleteParticipant(participant.NHSNumber, true);
                    if (ret == false)
                    {
                        return false;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Blank out the participant details and leave their record as simply an NHS number and deleted flag
        /// </summary>
        /// <param name="id">NHS number</param>
        /// <param name="removeNHSNumber">true - study number replaces NHS number, false - NHS number remains in place.</param>
        /// <returns>true if successfully deleted, else false</returns>
        public bool DeleteParticipant(string id, bool removeNHSNumber)
        {
            try
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == id).FirstOrDefault();
                if (participant != null)
                {
                    // blank everything!
                    participant.AttendedScreening = false;
                    participant.BMI = 0.00;
                    participant.Chemoprevention = false;
                    participant.ChemoPreventionDetails = null;
                    participant.ChemoPreventionDetailsId = null;
                    participant.InitialScreeningOutcome = null;
                    participant.InitialScreeningOutcomeId = null;
                    participant.FinalTechnicalOutcome = null;
                    participant.FinalAssessmentOutcome = null;
                    participant.ChemoAgreedInClinic = false;
                    participant.Consented = false;
                    participant.DateActualAppointment = null;
                    participant.DateFirstAppointment = null;
                    participant.DateConsented = null;
                    participant.DateOfBirth = null;
                    participant.Deceased = false;
                    participant.Deleted = true;
                    participant.Diagnosed = false;
                    participant.FHCReferral = false;
                    participant.MoreFrequentScreening = false;
                    participant.FirstName = null;
                    participant.GPName = null;
                    participant.LastName = null;
                    participant.MailingList = false;
                    participant.ScreeningNumber = null;
                    participant.ScreeningSite = null;
                    participant.SentRisk = false;
                    participant.Title = null;
                    participant.Withdrawn = false;
                    participant.AskForRiskLetter = false;
                    participant.RiskConsultationType = null;
                    participant.RiskConsultationBooked = false;
                    participant.RiskConsultationComments = null;
                    participant.RiskConsultationCompleted = false;
                    participant.RiskConsultationEligible = false;
                    participant.RiskConsultationLetterSent = false;
                    participant.FinalAssessmentOutcomeId = null;
                    participant.FinalTechnicalOutcomeId = null;
                    participant.RiskConsultationTypeId = null;

                    if (removeNHSNumber == true)
                    {
                        participant.NHSNumber = participant.StudyNumber.ToString().PadLeft(5, '0');
                    }

                    

                    _participantRepo.Update(participant);
                    _unitOfWork.Save();

                    _auditService.AddEvent(participant, _userManager.GetCurrentUser(), DateTime.Now, EventResources.EVENT_DELETED, EventResources.EVENT_DELETED_STR);
               

                    List<Address> addresses = _addressRepo.GetAll().Where(x => x.Participant.NHSNumber == id).ToList();
                    foreach (Address address in addresses)
                    {
                        _addressRepo.Delete(address);
                        _unitOfWork.Save();
                    }

                    List<RiskLetter> riskLetters = _riskLetterRepo.GetAll().Where(x => x.Participant.NHSNumber == id).ToList();
                    foreach(RiskLetter letter in riskLetters)
                    {
                        _riskLetterRepo.Delete(letter);
                        _unitOfWork.Save();
                    }

                    List<ScreeningRecordV1_5_4> screenings = _screeningRepo.GetAll().Where(x => x.Participant.NHSNumber == id).ToList();
                    foreach(ScreeningRecordV1_5_4 screening in screenings)
                    {
                        _screeningRepo.Delete(screening);
                        _unitOfWork.Save();
                    }

                    List<Image> images = _imageRepo.GetAll().Where(x => x.Participant.NHSNumber == id).ToList();
                    foreach(Image image in images)
                    {
                        _imageRepo.Delete(image);
                        _unitOfWork.Save();
                    }

                    List<VolparaDensity> densities = _densityRepo.GetAll().Where(x => x.Participant.NHSNumber == id).ToList();
                    foreach(VolparaDensity density in densities)
                    {
                        _densityRepo.Delete(density);
                        _unitOfWork.Save();
                    }

                    List<QuestionnaireResponseItem> responseItems = _questionnaireItemRepo.GetAll().Where(x => x.QuestionnaireResponse.Participant.NHSNumber == id).ToList();
                    foreach(QuestionnaireResponseItem responseItem in responseItems)
                    {
                        _questionnaireItemRepo.Delete(responseItem);
                        _unitOfWork.Save();
                    }

                    List<FamilyGeneticTestingItem> familyGenItems = _familyGeneticRepo.GetAll().Where(x => x.QuestionnaireResponse.Participant.NHSNumber == id).ToList();
                    foreach(FamilyGeneticTestingItem familyGenItem in familyGenItems)
                    {
                        _familyGeneticRepo.Delete(familyGenItem);
                        _unitOfWork.Save();
                    }

                    List<FamilyHistoryItem> familyHistoryItems = _familyHistoryRepo.GetAll().Where(x => x.QuestionnaireResponse.Participant.NHSNumber == id).ToList();
                    foreach(FamilyHistoryItem familyHistItem in familyHistoryItems)
                    {
                        _familyHistoryRepo.Delete(familyHistItem);
                        _unitOfWork.Save();
                    }

                    List<QuestionnaireResponse> responses = _questionnaireRepo.GetAll().Where(x => x.Participant.NHSNumber == id).ToList();
                    foreach (QuestionnaireResponse response in responses)
                    {
                        _questionnaireRepo.Delete(response);
                        _unitOfWork.Save();
                    }

                    _histologyService.DeleteHistology(id, 1);
                    _histologyService.DeleteHistology(id, 2);
                    

  
                }
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }


       

        /// <summary>
        /// Does the passed NHS number match a participant in the database?
        /// </summary>
        /// <param name="NHSNumber">NHS number</param>
        /// <returns>true if exists, else false</returns>
        public bool DoesNHSNumberExist(string NHSNumber)
        {
            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
            if (participant == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Does the passed hash match a participant in the database?
        /// </summary>
        /// <param name="hash">Hashed NHS Number</param>
        /// <returns>true if exists, else false</returns>
        public bool DoesHashedNHSNumberExist(string hash)
        {
            Participant participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hash).FirstOrDefault();
            if (participant == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Return a list of lookups of the type passed.
        /// </summary>
        /// <param name="lookupType">type of lookup</param>
        /// <returns>list of lookups</returns>
        public List<ParticipantLookup> GetLookups(string lookupType)
        {
            List<ParticipantLookup> list = new List<ParticipantLookup>();

            list = _lookupRepo.GetAll().Where(x => x.LookupType == lookupType).ToList();

            return list;
        }


        /// <summary>
        /// Create and return the next study number 
        /// </summary>
        /// <returns>The zero-padded study number</returns>
        public int CreateNextStudyNumber()
        {
            int currentHighest = 0;

            if (_participantRepo.GetAll().Count() > 0)
            {
                currentHighest = _participantRepo.GetAll().Max(x => x.StudyNumber);
            }
            
            // At time of initial development MaxStudyNumber is 99999. Unlikely to have more than 100000 participants!
            // The fewer the number of digits the better, as they need to use this number for searching in CRA Health
            if (currentHighest == Convert.ToInt32(_configService.GetAppSetting("MaxStudyNumber")))
            {
                return 0; 
            }
            else
            {
                return currentHighest + 1;
            }
        }

        /// <summary>
        /// Get the participant history and put it in the view model
        /// </summary>
        /// <param name="NHSNumber">NHS number</param>
        /// <param name="model">reference to the view model</param>
        /// <returns>true if the patient is found, else false</returns>
        public bool GetParticipantHistory(string NHSNumber, ref ParticipantHistoryDetailsViewModel model)
        {
            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
            if (participant != null)
            {
                model.NHSNumber = NHSNumber;
                model.StudyNumber = participant.StudyNumber;
                model.Events = participant.ParticipantEvents.ToList();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set the patient's consented flag to true
        /// </summary>
        /// <param name="hashedNHSNumber">Hashed NHS Number</param>
        /// <returns>true if set OK, else false</returns>
        public bool SetConsentFlag(string hashedNHSNumber, DateTime dateOfConsent)
        {
            try
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.HashedNHSNumber == hashedNHSNumber).FirstOrDefault();
                if (participant != null)
                {
                   
                    participant.Consented = true;
                    participant.DateConsented = _auditService.ChangeEventDate(participant, ParticipantResources.DATE_CONSENTED, participant.DateConsented, dateOfConsent, "");
                    _participantRepo.Update(participant);
                   
                    _unitOfWork.Save();

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

    }
}
