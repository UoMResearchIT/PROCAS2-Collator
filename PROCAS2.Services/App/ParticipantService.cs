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
        private IUnitOfWork _unitOfWork;
        private IPROCAS2UserManager _userManager;
        private IHashingService _hashingService;

        public ParticipantService(IUnitOfWork unitOfWork,
                                IGenericRepository<Participant> participantRepo,
                                IGenericRepository<ParticipantEvent> eventRepo,
                                IPROCAS2UserManager userManager,
                                IGenericRepository<EventType> eventTypeRepo,
                                IHashingService hashingService)
        {
            _unitOfWork = unitOfWork;
            _participantRepo = participantRepo;
            _eventRepo = eventRepo;
            _userManager = userManager;
            _eventTypeRepo = eventTypeRepo;
            _hashingService = hashingService;
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
                return false;
            }


            
        }


        /// <summary>
        /// Create a new participant record
        /// </summary>
        /// <param name="NHSNumber">NHS number</param>
        /// <returns>Hashed NHS number</returns>
        public string CreateNewParticipantRecord(string NHSNumber)
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
            pEvent.EventType = _eventTypeRepo.GetAll().Where(x => x.Code == PROCASRes.EVENT_CREATED).FirstOrDefault();
            pEvent.Notes = PROCASRes.EVENT_CREATED_STR;
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
            if (lineBits.Count() != 1) // Should only be 1 column
            {
                
                outModel.Messages.Add("Line " + lineCount + ": " + string.Format(PROCASRes.UPLOAD_INCORRECT_COLUMNS, 1));
                return false;
            }

            string NHSNumber = lineBits[0];

            if (lineBits[0].Length > 12) // NHS numbers are in fact 10 characters long, but DB column was given 12 chars to allow for expansion
            {
               
                outModel.Messages.Add("Line " + lineCount + ": " + string.Format(PROCASRes.UPLOAD_NHS_NUMBER_TOO_LONG, NHSNumber));
                return false;
            }

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
            if (participant != null) // Participant already exists in the database
            {
                
                outModel.Messages.Add("Line " + lineCount + ": " + string.Format(PROCASRes.UPLOAD_NHS_NUMBER_IN_DB, NHSNumber));
                return false;
            }

            return true;

        }
    }
}
