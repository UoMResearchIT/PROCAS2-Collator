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
        private IUnitOfWork _unitOfWork;
        private IPROCAS2UserManager _userManager;
        private IHashingService _hashingService;
        private IConfigService _configService;

       

        private int _UPLOADNEWCOLUMNS;
        private int _UPLOADUPDATECOLUMNS;
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
                                IGenericRepository<Address> addressRepo)
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

            // Get the config settings for the uploading. Defaults are deliberately set to be stupid values, to make
            // sure that you set them in the config!
            _UPLOADNEWCOLUMNS = _configService.GetIntAppSetting("UploadNewColumns") ?? 0;
            _UPLOADUPDATECOLUMNS = _configService.GetIntAppSetting("UploadUpdateColumns") ?? 0;
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
            if (lineBits.Count() == _UPLOADUPDATECOLUMNS) // Should be 23 columns
            {
                string NHSNumber = lineBits[0];
                Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
                if (participant != null)
                {
                    DateTime DOFA = Convert.ToDateTime(lineBits[3]);
                    DateTime DOB = Convert.ToDateTime(lineBits[2]);

                    participant.ScreeningNumber = lineBits[1];
                    participant.DateFirstAppointment = DOFA;
                    participant.DateOfBirth = DOB;
                    participant.Title = lineBits[4];
                    participant.FirstName = lineBits[5];
                    participant.LastName = lineBits[6];
                    participant.BMI = Convert.ToInt32(lineBits[7]);
                    participant.MailingList = lineBits[8] == "Y" ? true : false;
                    participant.GPName = lineBits[15];

                    string siteCode = lineBits[22];
                    ScreeningSite site = _siteRepo.GetAll().Where(x => x.Code == siteCode).FirstOrDefault();
                    if (site != null)
                    {
                        participant.ScreeningSite = site;
                    }

                    _participantRepo.Update(participant);
                    _unitOfWork.Save();

                    Address homeAddress = new Address();
                    homeAddress.AddressLine1 = lineBits[9];
                    homeAddress.AddressLine2 = lineBits[10];
                    homeAddress.AddressLine3 = lineBits[11];
                    homeAddress.AddressLine4 = lineBits[12];
                    homeAddress.PostCode = lineBits[13];
                    homeAddress.EmailAddress = lineBits[14];
                    homeAddress.Participant = participant;
                    homeAddress.AddressType = _addressTypeRepo.GetAll().Where(x => x.Name == "HOME").FirstOrDefault();

                    _addressRepo.Insert(homeAddress);
                    _unitOfWork.Save();

                    Address gpAddress = new Address();
                    gpAddress.AddressLine1 = lineBits[16];
                    gpAddress.AddressLine2 = lineBits[17];
                    gpAddress.AddressLine3 = lineBits[18];
                    gpAddress.AddressLine4 = lineBits[19];
                    gpAddress.PostCode = lineBits[20];
                    gpAddress.EmailAddress = lineBits[21];
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

            // BMI
            int BMI;
            if (String.IsNullOrEmpty(lineBits[7]) == false && Int32.TryParse(lineBits[7], out BMI) == false) // check out the format
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_BMI_WRONG_FORMAT), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Mailing list
            if (String.IsNullOrEmpty(lineBits[8]) == false && (lineBits[8]!= "N" && lineBits[8] != "Y")) // check out the format
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_MAILING_LIST_WRONG_FORMAT), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Address line 1
            if (String.IsNullOrEmpty(lineBits[9]) == true) // First address line is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_1_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[9].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine1)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_1_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }


            // Address line 2
            if (lineBits[10].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine2)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_2_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Address line 3
            if (lineBits[11].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine3)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_3_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Address line 4
            if (lineBits[12].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine4)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_ADDRESS_4_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Postcode
            if (String.IsNullOrEmpty(lineBits[13]) == true) // Postcode is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_POSTCODE_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[13].Length > AttributeHelpers.GetMaxLength<Address>(x => x.PostCode)) // should be 10 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_POSTCODE_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Email address
            if (lineBits[14].Length > AttributeHelpers.GetMaxLength<Address>(x => x.EmailAddress)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_EMAIL_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // GP Name
            if (String.IsNullOrEmpty(lineBits[15]) == true) // GP Name is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_NAME_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[15].Length > AttributeHelpers.GetMaxLength<Participant>(x => x.GPName)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_NAME_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }


            // GP Address line 1
            if (String.IsNullOrEmpty(lineBits[16]) == true) // First GP address line is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_1_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[16].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine1)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_1_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }


            // GP Address line 2
            if (lineBits[17].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine2)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_2_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // GP Address line 3
            if (lineBits[18].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine3)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_3_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // GP Address line 4
            if (lineBits[19].Length > AttributeHelpers.GetMaxLength<Address>(x => x.AddressLine4)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_ADDRESS_4_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // GP Postcode
            if (String.IsNullOrEmpty(lineBits[20]) == true) // GP Postcode is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_POSTCODE_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[20].Length > AttributeHelpers.GetMaxLength<Address>(x => x.PostCode)) // should be 10 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_POSTCODE_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // GP Email address
            if (lineBits[21].Length > AttributeHelpers.GetMaxLength<Address>(x => x.EmailAddress)) // should be 200 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_GP_EMAIL_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            // Site code
            if (String.IsNullOrEmpty(lineBits[22]) == true) // Site code is mandatory
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_SITE_CODE_EMPTY), UploadResources.UPLOAD_FAIL);
                return false;
            }

            if (lineBits[22].Length > AttributeHelpers.GetMaxLength<ScreeningSite>(x => x.Code)) // should be 10 chars max
            {
                outModel.AddMessage(lineCount, string.Format(UploadResources.UPLOAD_SITE_CODE_TOO_LONG), UploadResources.UPLOAD_FAIL);
                return false;
            }

            string siteCode = lineBits[22];
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

    }
}
