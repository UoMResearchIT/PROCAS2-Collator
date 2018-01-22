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
        private IUnitOfWork _unitOfWork;
        private IPROCAS2UserManager _userManager;
        private IHashingService _hashingService;
        private IConfigService _configService;

       

        private int _UPLOADNEWCOLUMNS;
        private int _UPLOADUPDATECOLUMNS;
        private int _UPLOADASKRISKCOLUMNS;
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
                                IGenericRepository<Image> imageRepo)
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

            // Get the config settings for the uploading. Defaults are deliberately set to be stupid values, to make
            // sure that you set them in the config!
            _UPLOADNEWCOLUMNS = _configService.GetIntAppSetting("UploadNewColumns") ?? 0;
            _UPLOADUPDATECOLUMNS = _configService.GetIntAppSetting("UploadUpdateColumns") ?? 0;
            _UPLOADASKRISKCOLUMNS = _configService.GetIntAppSetting("UploadAskRiskColumns") ?? 0;
            _EARLIESTDOB = _configService.GetDateTimeAppSetting("EarliestDOB") ?? DateTime.Now;
            _LATESTDOB = _configService.GetDateTimeAppSetting("LatestDOB") ?? DateTime.Now;
            _EARLIESTDOFA = _configService.GetDateTimeAppSetting("EarliestDOFA") ?? DateTime.Now;
            _LATESTDOFA = _configService.GetDateTimeAppSetting("LatestDOFA") ?? DateTime.Now;
        }

        /// <summary>
        /// Upload a CSV file of new NHS numbers, to create a new placeholder participant record (ready for consent)
        /// </summary>
        /// <param name="model">View model containing the CSV file</param>
        /// <param name="outModel">New view model with any error messages</param>
        /// <param name="hashFile">Memory stream containing the hashes of the NHS numbers.</param>
        /// <returns>true if all the entries in the CSV file are uploaded, else false</returns>
        public bool UploadNewParticipants( UploadNewParticipantsViewModel model,  out UploadResultsViewModel outModel, out MemoryStream hashFile)
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

                valid = ValidateNewParticipantLine(line, lineCount, ref outModel);
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
                    
                    csv.Configuration.Delimiter = ",";

                    
                    while (reader.EndOfStream == false)
                    {
                        string line = reader.ReadLine();
                        string[] lineBits = line.Split(',');


                        string hash = CreateNewParticipantRecord(lineBits[0]);

                        // First put in the NHSNumber.
                        csv.WriteField(lineBits[0]);
                        // Then the hash
                        csv.WriteField(hash);
                        csv.NextRecord();
                    }
                }

                hashFile = new MemoryStream(Encoding.UTF8.GetBytes(csvString.ToString()));
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

                        
                        ParticipantEvent pEvent = new ParticipantEvent();
                        pEvent.AppUser = _userManager.GetCurrentUser();
                        pEvent.EventDate = DateTime.Now;
                        pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_ASK_RISK).FirstOrDefault();
                        pEvent.Notes = EventResources.EVENT_ASK_RISK_STR;
                        pEvent.Participant = participant;

                        _eventRepo.Insert(pEvent);
                        _unitOfWork.Save();


                        participant.LastEvent = pEvent;
                        _participantRepo.Update(participant);

                        _unitOfWork.Save();

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
        /// Create a new participant record
        /// </summary>
        /// <param name="NHSNumber">NHS number</param>
        /// <returns>Hashed NHS number</returns>
        private string CreateNewParticipantRecord(string NHSNumber)
        {
            DateTime dateCreated = DateTime.Now;

            Participant participant = new Participant();
            participant.NHSNumber = NHSNumber;
            string hash = _hashingService.CreateHash(NHSNumber);
            participant.HashedNHSNumber = hash;
            participant.DateCreated = dateCreated;
            _participantRepo.Insert(participant);

            _unitOfWork.Save();

            ParticipantEvent pEvent = new ParticipantEvent();
            pEvent.AppUser = _userManager.GetCurrentUser();
            pEvent.EventDate = dateCreated;
            pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_CREATED).FirstOrDefault();
            pEvent.Notes = EventResources.EVENT_CREATED_STR;
            pEvent.Participant = participant;

            _eventRepo.Insert(pEvent);
            _unitOfWork.Save();


            participant.LastEvent = pEvent;
            _participantRepo.Update(participant);

            _unitOfWork.Save();

            return hash;
        }

        /// <summary>
        /// Check the the passed line is valid
        /// </summary
        /// <param name="line">The line string</param>
        /// <param name="lineCount">Line number</param>
        /// <param name="outModel">View model to add result messages to</param>
        /// <returns>true if valid, else false</returns>
        private bool ValidateNewParticipantLine(string line, int lineCount, ref UploadResultsViewModel outModel)
        {
            
            string[] lineBits = line.Split(',');
            if (lineBits.Count() != _UPLOADNEWCOLUMNS) // Should only be 1 column
            {
                
                outModel.AddMessage(lineCount , string.Format(UploadResources.UPLOAD_INCORRECT_COLUMNS, 1), UploadResources.UPLOAD_FAIL);
                return false;
            }

            string NHSNumber = lineBits[0];

            if (lineBits[0].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.NHSNumber)) // NHS numbers are in fact 10 characters long, but DB column was given 12 chars to allow for expansion
            {
               
                outModel.AddMessage(lineCount , string.Format(UploadResources.UPLOAD_NHS_NUMBER_TOO_LONG, NHSNumber), UploadResources.UPLOAD_FAIL);
                return false;
            }

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
            if (participant != null) // Participant already exists in the database
            {
                
                outModel.AddMessage(lineCount , string.Format(UploadResources.UPLOAD_NHS_NUMBER_IN_DB, NHSNumber), UploadResources.UPLOAD_FAIL);
                return false;
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

                    ParticipantEvent pEvent = new ParticipantEvent();
                    pEvent.AppUser = _userManager.GetCurrentUser();
                    pEvent.EventDate = DateTime.Now;
                    pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_UPDATED).FirstOrDefault();
                    pEvent.Notes = EventResources.EVENT_UPDATED_STR;
                    pEvent.Participant = participant;

                    _eventRepo.Insert(pEvent);
                    _unitOfWork.Save();


                    participant.LastEvent = pEvent;
                    _participantRepo.Update(participant);

                    _unitOfWork.Save();

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
                        participant.BMI = ChangeEventInt(participant, ParticipantResources.BMI, participant.BMI, Convert.ToInt32(model.BMI), model.Reason);
                    }
                    else
                    {
                        participant.BMI = ChangeEventInt(participant, ParticipantResources.BMI, participant.BMI, null, model.Reason);

                    }
                    participant.Chemoprevention = ChangeEventBool(participant, ParticipantResources.CHEMO , participant.Chemoprevention, model.Chemo, model.Reason);
                    participant.Consented = ChangeEventBool(participant, ParticipantResources.CONSENTED, participant.Consented, model.Consented, model.Reason);
                    participant.DateActualAppointment = ChangeEventDate(participant, ParticipantResources.DOAA, (DateTime)participant.DateActualAppointment, (DateTime)model.DOAA, model.Reason);
                    participant.DateFirstAppointment = ChangeEventDate(participant, ParticipantResources.DOFA, (DateTime)participant.DateFirstAppointment, (DateTime)model.DOFA, model.Reason);
                    participant.DateOfBirth = ChangeEventDate(participant, ParticipantResources.DOB, (DateTime)participant.DateOfBirth, (DateTime)model.DOB, model.Reason);
                    participant.Deceased = ChangeEventBool(participant, ParticipantResources.DECEASED, participant.Deceased, model.Deceased, model.Reason);
                    participant.Diagnosed = ChangeEventBool(participant, ParticipantResources.DIAGNOSED, participant.Diagnosed, model.Diagnosed, model.Reason);
                    participant.FHCReferral = ChangeEventBool(participant, ParticipantResources.FHC_REFERRAL, participant.FHCReferral, model.FHCReferral, model.Reason);
                    participant.FirstName = ChangeEventString(participant, ParticipantResources.FIRST_NAME, participant.FirstName, model.FirstName, model.Reason);
                    participant.GPName = ChangeEventString(participant, ParticipantResources.GP_NAME, participant.GPName, model.GPName, model.Reason);
                    participant.LastName = ChangeEventString(participant, ParticipantResources.LAST_NAME, participant.LastName, model.LastName, model.Reason);
                    participant.ScreeningNumber = ChangeEventString(participant, ParticipantResources.SCREENING_NUMBER, participant.ScreeningNumber, model.ScreeningNumber, model.Reason);

                    ScreeningSite newSite = _siteRepo.GetAll().Where(x => x.Code == model.ScreeningSite).FirstOrDefault();
                    ChangeEventString(participant, ParticipantResources.SCREENING_SITE, participant.ScreeningSite.Code, newSite.Code, model.Reason);
                    participant.ScreeningSite = newSite;
                    
                    participant.SentRisk = ChangeEventBool(participant, ParticipantResources.SENT_RISK, participant.SentRisk, model.SentRisk, model.Reason);
                    participant.AttendedScreening = ChangeEventBool(participant, ParticipantResources.ATTENDED_SCREENING, participant.AttendedScreening, model.AttendedScreening, model.Reason);
                    participant.Title = ChangeEventString(participant, ParticipantResources.TITLE, participant.Title, model.Title, model.Reason);
                    participant.Withdrawn = ChangeEventBool(participant, ParticipantResources.WITHDRAWN, participant.Withdrawn, model.Withdrawn, model.Reason);
                    participant.MailingList = ChangeEventBool(participant, ParticipantResources.MAILING_LIST, participant.MailingList, model.MailingList, model.Reason);
                    participant.AskForRiskLetter = ChangeEventBool(participant, ParticipantResources.ASKFORRISK, participant.AskForRiskLetter, model.AskForRiskLetter, model.Reason);


                    _participantRepo.Update(participant);
                    _unitOfWork.Save();

                    Address homeAdd = _addressRepo.GetAll().Where(x => x.Participant.NHSNumber == model.NHSNumber && x.AddressType.Name == "HOME").FirstOrDefault();
                    if (homeAdd != null)
                    {
                        homeAdd.AddressLine1 = ChangeEventString(participant, ParticipantResources.HOME_ADD_1, homeAdd.AddressLine1, model.HomeAddress1, model.Reason);
                        homeAdd.AddressLine2 = ChangeEventString(participant, ParticipantResources.HOME_ADD_2, homeAdd.AddressLine2, model.HomeAddress2, model.Reason);
                        homeAdd.AddressLine3 = ChangeEventString(participant, ParticipantResources.HOME_ADD_3, homeAdd.AddressLine3, model.HomeAddress3, model.Reason);
                        homeAdd.AddressLine4 = ChangeEventString(participant, ParticipantResources.HOME_ADD_4, homeAdd.AddressLine4, model.HomeAddress4, model.Reason);
                        homeAdd.EmailAddress = ChangeEventString(participant, ParticipantResources.HOMEEMAIL, homeAdd.EmailAddress, model.HomeEmail, model.Reason);
                        homeAdd.PostCode = ChangeEventString(participant, ParticipantResources.HOME_POSTCODE, homeAdd.PostCode, model.HomePostCode, model.Reason);
                        _addressRepo.Update(homeAdd);
                        _unitOfWork.Save();
                    }

                    Address gpAdd = _addressRepo.GetAll().Where(x => x.Participant.NHSNumber == model.NHSNumber && x.AddressType.Name == "GP").FirstOrDefault();
                    if (gpAdd != null)
                    {
                        gpAdd.AddressLine1 = ChangeEventString(participant, ParticipantResources.GP_ADD_1, gpAdd.AddressLine1, model.GPAddress1, model.Reason);
                        gpAdd.AddressLine2 = ChangeEventString(participant, ParticipantResources.GP_ADD_2, gpAdd.AddressLine2, model.GPAddress2, model.Reason);
                        gpAdd.AddressLine3 = ChangeEventString(participant, ParticipantResources.GP_ADD_3, gpAdd.AddressLine3, model.GPAddress3, model.Reason);
                        gpAdd.AddressLine4 = ChangeEventString(participant, ParticipantResources.GP_ADD_4, gpAdd.AddressLine4, model.GPAddress4, model.Reason);
                        gpAdd.EmailAddress = ChangeEventString(participant, ParticipantResources.GP_EMAIL, gpAdd.EmailAddress, model.GPEmail, model.Reason);
                        gpAdd.PostCode = ChangeEventString(participant, ParticipantResources.GP_POSTCODE, gpAdd.PostCode, model.GPPostCode, model.Reason);
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
        /// Blank out the participant details and leave their record as simply an NHS number and deleted flag
        /// </summary>
        /// <param name="id">NHS number</param>
        /// <returns>true if successfully deleted, else false</returns>
        public bool DeleteParticipant(string id)
        {
            try
            {
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == id).FirstOrDefault();
                if (participant != null)
                {
                    // blank everything!
                    participant.AttendedScreening = false;
                    participant.BMI = 0;
                    participant.Chemoprevention = false;
                    participant.Consented = false;
                    participant.DateActualAppointment = null;
                    participant.DateFirstAppointment = null;
                    participant.DateOfBirth = null;
                    participant.Deceased = false;
                    participant.Deleted = true;
                    participant.Diagnosed = false;
                    participant.FHCReferral = false;
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

                    _participantRepo.Update(participant);
                    _unitOfWork.Save();

                    ParticipantEvent pEvent = new ParticipantEvent();
                    pEvent.AppUser = _userManager.GetCurrentUser();
                    pEvent.EventDate = DateTime.Now;
                    pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_DELETED).FirstOrDefault();
                    pEvent.Notes = EventResources.EVENT_DELETED_STR;
                    pEvent.Participant = participant;
                    _eventRepo.Insert(pEvent);
                    _unitOfWork.Save();

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

                    // TODO: delete other records too!

                    participant.LastEvent = pEvent;
                    _participantRepo.Update(participant);

                    _unitOfWork.Save();

                }
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Create a audit trail event if the value is changed. For boolean properties
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="participant">Participant object</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns></returns>
        private bool ChangeEventBool (Participant participant, string propertyName, bool oldValue, bool newValue, string reason)
        {
            if (oldValue != newValue)
            {
                ParticipantEvent pEvent = new ParticipantEvent();
                
                pEvent.AppUser = _userManager.GetCurrentUser();
                pEvent.EventDate = DateTime.Now;
                pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_PROPERTY_UPDATED).FirstOrDefault();
                pEvent.Notes = String.Format(EventResources.EVENT_PROPERTY_UPDATED_STR, propertyName, oldValue.ToString(), newValue.ToString());
                pEvent.Reason = reason;
                pEvent.Participant = participant;
                _eventRepo.Insert(pEvent);
                _unitOfWork.Save();

                participant.LastEvent = pEvent;
                _participantRepo.Update(participant);

                _unitOfWork.Save();
            }

            return newValue;
        }

        /// <summary>
        /// Create a audit trail event if the value is changed. For string properties
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="participant">Participant object</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns></returns>
        private string ChangeEventString(Participant participant, string propertyName, string oldValue, string newValue, string reason)
        {
            if (oldValue != newValue)
            {
                ParticipantEvent pEvent = new ParticipantEvent();

                pEvent.AppUser = _userManager.GetCurrentUser();
                pEvent.EventDate = DateTime.Now;
                pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_PROPERTY_UPDATED).FirstOrDefault();
                pEvent.Notes = String.Format(EventResources.EVENT_PROPERTY_UPDATED_STR, propertyName, oldValue, newValue);
                pEvent.Participant = participant;
                pEvent.Reason = reason;
                _eventRepo.Insert(pEvent);
                _unitOfWork.Save();

                participant.LastEvent = pEvent;
                _participantRepo.Update(participant);

                _unitOfWork.Save();
            }

            return newValue;
        }

        /// <summary>
        /// Create a audit trail event if the value is changed. For date properties
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="participant">Participant object</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns></returns>
        private DateTime ChangeEventDate(Participant participant, string propertyName, DateTime oldValue, DateTime newValue, string reason)
        {
            if (oldValue != newValue)
            {
                ParticipantEvent pEvent = new ParticipantEvent();

                pEvent.AppUser = _userManager.GetCurrentUser();
                pEvent.EventDate = DateTime.Now;
                pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_PROPERTY_UPDATED).FirstOrDefault();
                pEvent.Notes = String.Format(EventResources.EVENT_PROPERTY_UPDATED_STR, propertyName, oldValue.ToString("dd/MM/yyyy"), newValue.ToString("dd/MM/yyyy"));
                pEvent.Participant = participant;
                pEvent.Reason = reason;
                _eventRepo.Insert(pEvent);
                _unitOfWork.Save();

                participant.LastEvent = pEvent;
                _participantRepo.Update(participant);

                _unitOfWork.Save();
            }

            return newValue;
        }

        /// <summary>
        /// Create a audit trail event if the value is changed. For integer properties
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="participant">Participant object</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns></returns>
        private int? ChangeEventInt(Participant participant, string propertyName, int? oldValue, int? newValue, string reason)
        {
            if (oldValue != newValue)
            {
                ParticipantEvent pEvent = new ParticipantEvent();

                pEvent.AppUser = _userManager.GetCurrentUser();
                pEvent.EventDate = DateTime.Now;
                pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == EventResources.EVENT_PROPERTY_UPDATED).FirstOrDefault();
                pEvent.Notes = String.Format(EventResources.EVENT_PROPERTY_UPDATED_STR, propertyName, oldValue.HasValue?oldValue.ToString(): "NULL", newValue.HasValue?newValue.ToString():"NULL");
                pEvent.Participant = participant;
                pEvent.Reason = reason;
                _eventRepo.Insert(pEvent);
                _unitOfWork.Save();

                participant.LastEvent = pEvent;
                _participantRepo.Update(participant);

                _unitOfWork.Save();
            }

            return newValue;
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

    }
}
