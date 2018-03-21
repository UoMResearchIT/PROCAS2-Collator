using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;
using System.Data.Entity;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using PROCAS2.Data;
using PROCAS2.Data.Entities;


namespace PROCAS2.Services.App
{
    public class ReportService : IReportService
    {

        private IGenericRepository<Participant> _participantRepo;
        private IGenericRepository<Histology> _histologyRepo;

        public ReportService(IGenericRepository<Participant> participantRepo,
                IGenericRepository<Histology> histologyRepo)
        {
            _participantRepo = participantRepo;
            _histologyRepo = histologyRepo;
        }


        #region SPREADSHEET_FUNCS
        /// <summary>
        /// Add the workbook to the spreadsheet
        /// </summary>
        /// <param name="spreadDoc"></param>
        /// <returns>The workbook part</returns>
        private WorkbookPart AddWorkbookPart(SpreadsheetDocument spreadDoc)
        {
            WorkbookPart wbPart = spreadDoc.WorkbookPart;
            if (wbPart == null)
            {
                wbPart = spreadDoc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();
            }


            if (wbPart.Workbook.Sheets == null)
            {
                wbPart.Workbook.AppendChild<Sheets>(new Sheets());
            }

            return wbPart;
        }

        /// <summary>
        /// Add a sheet to the workbook.
        /// </summary>
        /// <param name="wbPart">workbookpart to add the sheet to</param>
        /// <param name="sheetName">Title of the sheet</param>
        /// <param name="sheetId">External ID of sheet</param>
        /// <returns>ID of the sheet</returns>
        private string AddSheet(WorkbookPart wbPart, string sheetName, uint sheetId)
        {
            WorksheetPart worksheetPart = null;
            worksheetPart = wbPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();

            worksheetPart.Worksheet = new Worksheet(sheetData);


            var sheet = new Sheet()
            {
                Id = wbPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId,
                Name = sheetName
            };

            wbPart.Workbook.Sheets.AppendChild(sheet);
            return sheet.Id;
        }

        /// <summary>
        /// Create a cell from the passed text
        /// </summary>
        /// <param name="text">text to put in cell</param>
        /// <returns>the cell</returns>
        private Cell AddCellWithText(string text)
        {
            Cell c1 = new Cell();
            c1.DataType = CellValues.InlineString;

            InlineString inlineString = new InlineString();
            Text t = new Text();
            t.Text = text;
            inlineString.AppendChild(t);

            c1.AppendChild(inlineString);

            return c1;
        }

        /// <summary>
        /// Add a header at the top of the worksheet, based on the properties of the passed type
        /// </summary>
        /// <param name="activeWorksheet">The worksheet to add a header to</param>
        /// <param name="type">a system type</param>
        /// <param name="rowindex">row to put the header on</param>
        /// <param name="beforeCols">Column headers to add before main list of properties (e.g. an ID etc)</param>
        /// <param name="afterCols">Column headers to add after main list of properties</param>
        /// <param name="onlyCols">Columns to include (will only have these columns if not null)</param>
        private void AddHeaderFromProperties(Worksheet activeWorksheet, System.Type type, int rowindex,
                                            List<string> beforeCols = null,
                                            List<string> afterCols = null,
                                            List<string> onlyCols = null)
        {
            Row headerRow = new Row();
            headerRow.RowIndex = (UInt32)rowindex;

            if (beforeCols != null)
            {
                foreach (string col in beforeCols)
                {
                    headerRow.AppendChild(AddCellWithText(col));
                }
            }

            foreach (PropertyInfo pi in type.GetProperties())
            {
                if (onlyCols == null || (onlyCols != null && onlyCols.Contains(pi.Name)))
                {
                    if (pi.Name.EndsWith("Id") == false)
                    {
                        if (pi.PropertyType == typeof(String) || pi.PropertyType == typeof(Int32) || pi.PropertyType == typeof(Boolean) ||
                            pi.PropertyType == typeof(int?) || pi.PropertyType == typeof(DateTime?) || pi.PropertyType == typeof(DateTime) ||
                            pi.PropertyType == typeof(double) || pi.PropertyType == typeof(double?))
                        {
                            headerRow.AppendChild(AddCellWithText(pi.Name));
                        }
                    }
                }
            }

            if (afterCols != null)
            {
                foreach (string col in afterCols)
                {
                    headerRow.AppendChild(AddCellWithText(col));
                }
            }

            activeWorksheet.Where(x => x.LocalName == "sheetData").First().AppendChild(headerRow);
        }

        /// <summary>
        /// Add a detail line, based on the properties of the passed type
        /// </summary>
        /// <param name="activeWorksheet">The worksheet to add a header to</param>
        /// <param name="data">an object containing the properties to output</param>
        /// <param name="type">a system type</param>
        /// <param name="rowindex">row to put the header on</param>
        /// <param name="beforeCols">Column data to add before main list of properties (e.g. an ID etc)</param>
        /// <param name="afterCols">Column data to add after main list of properties</param>
        /// <param name="onlyCols">Columns to include (will only have these columns if not null)</param>
        private void AddLineFromProperties(Worksheet activeWorksheet, object data, System.Type type, int rowindex,
                                            List<string> beforeCols = null,
                                            List<string> afterCols = null,
                                            List<string> onlyCols = null)
        {
            Row lineRow = new Row();
            lineRow.RowIndex = (UInt32)rowindex;

            if (beforeCols != null)
            {
                foreach (string col in beforeCols)
                {
                    lineRow.AppendChild(AddCellWithText(col));
                }
            }

            foreach (PropertyInfo pi in type.GetProperties())
            {
                if (onlyCols == null || (onlyCols != null && onlyCols.Contains(pi.Name)))
                {
                    if (pi.Name.EndsWith("Id") == false)
                    {
                        if (pi.PropertyType == typeof(String) || pi.PropertyType == typeof(Int32) || pi.PropertyType == typeof(Boolean) ||
                            pi.PropertyType == typeof(int?) || pi.PropertyType == typeof(DateTime?) || pi.PropertyType == typeof(DateTime) ||
                            pi.PropertyType == typeof(double) || pi.PropertyType == typeof(double?))
                        {
                            lineRow.AppendChild(AddCellWithText(pi.GetValue(data) == null ? "" : pi.GetValue(data).ToString()));
                        }
                    }
                }
            }

            if (afterCols != null)
            {
                foreach (string col in afterCols)
                {
                    lineRow.AppendChild(AddCellWithText(col));
                }
            }


            activeWorksheet.Where(x => x.LocalName == "sheetData").First().AppendChild(lineRow);
        }

       


        #endregion

        /// <summary>
        /// Generate the patient report
        /// </summary>
        /// <param name="NHSNumbers">List of NHS numbers </param>
        /// <returns>MemoryStream of generated spreadsheet</returns>
        public MemoryStream PatientReport(List<string> NHSNumbers)
        {

            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);

                // Add participant header
                string mainSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(mainSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, afterCols: new List<string>() { "Site" });

                // Add address header
                string addressSheetId = AddSheet(wbPart, "Addresses", 2);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(addressSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Address), 1,
                                            beforeCols: new List<string>() { "NHSNumber" },
                                            afterCols: new List<string>() { "Type" });

                // Add Volpara header
                string volparaSheetId = AddSheet(wbPart, "Volpara", 3);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(volparaSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(ScreeningRecordV1_5_4), 1, beforeCols: new List<string>() { "NHSNumber" });

                // Add Risk letter header
                string riskSheetId = AddSheet(wbPart, "Risk", 4);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(riskSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(RiskLetter), 1, beforeCols: new List<string>() { "NHSNumber" });

                // Add Survey header
                string surveySheetId = AddSheet(wbPart, "SurveyHeader", 5);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(surveySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(QuestionnaireResponse), 1, beforeCols: new List<string>() { "NHSNumber", "ResponseId" });

                // Add Survey item header
                string surveyItemSheetId = AddSheet(wbPart, "SurveyItems", 6);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(surveyItemSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(QuestionnaireResponseItem), 1, beforeCols: new List<string>() { "NHSNumber", "ResponseId" },
                                                                                            afterCols: new List<string>() { "QuestionText" });

                // Add family history header
                string familyHistorySheetId = AddSheet(wbPart, "FamilyHistory", 7);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(familyHistorySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(FamilyHistoryItem), 1, beforeCols: new List<string>() { "NHSNumber", "ResponseId" });

                // Add Histology header
                string histologySheetId = AddSheet(wbPart, "Histology", 8);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(histologySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Histology), 1, beforeCols: new List<string>() { "NHSNumber", "DOB", "BMI", "RiskScore", "HistologyId" },
                                                                            afterCols: new List<string>() { "DiagnosisType", "DiagnosisSide" });

                // Add Histology focus header
                string histologyFocusSheetId = AddSheet(wbPart, "HistologyFocus", 9);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(histologyFocusSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(HistologyFocus), 1, beforeCols: new List<string>() { "NHSNumber", "HistologyId" },
                                                                            afterCols: new List<string>() { "Pathology", "Invasive", "DCISGrade", "VascularInvasion", "HER2Score", "TNMStage" });

                int parIndex = 2;
                int addressIndex = 2;
                int volparaIndex = 2;
                int riskIndex = 2;
                int surveyIndex = 2;
                int surveyItemIndex = 2;
                int familyHistoryIndex = 2;
                int histIndex = 2;
                int histFocusIndex = 2;


                foreach (string NHSNumber in NHSNumbers.OrderBy(x => x))
                {
                    // Add participant details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(mainSheetId)).Worksheet;
                    Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).First();
                    AddLineFromProperties(workingSheet, participant, typeof(Participant), parIndex, afterCols: new List<string>() { participant.ScreeningSite.Name });

                    // Add address details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(addressSheetId)).Worksheet;
                    foreach (Address address in participant.Addresses)
                    {
                        AddLineFromProperties(workingSheet, address, typeof(Address), addressIndex,
                                            beforeCols: new List<string>() { participant.NHSNumber },
                                            afterCols: new List<string>() { address.AddressType.Name });
                        addressIndex++;
                    }

                    // Add Volpara details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(volparaSheetId)).Worksheet;
                    foreach (ScreeningRecordV1_5_4 screening in participant.ScreeningRecordV1_5_4s)
                    {
                        AddLineFromProperties(workingSheet, screening, typeof(ScreeningRecordV1_5_4), volparaIndex,
                                            beforeCols: new List<string>() { participant.NHSNumber });
                        volparaIndex++;
                    }

                    // Add Risk Letter details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(riskSheetId)).Worksheet;
                    foreach (RiskLetter letter in participant.RiskLetters)
                    {
                        AddLineFromProperties(workingSheet, letter, typeof(RiskLetter), riskIndex,
                                            beforeCols: new List<string>() { participant.NHSNumber });
                        riskIndex++;
                    }


                    // Add survey header details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(surveySheetId)).Worksheet;
                    foreach (QuestionnaireResponse response in participant.QuestionnaireResponses)
                    {
                        AddLineFromProperties(workingSheet, response, typeof(QuestionnaireResponse), surveyIndex,
                                            beforeCols: new List<string>() { participant.NHSNumber, response.Id.ToString() });
                        surveyIndex++;
                    }

                    // Add survey item and famnily history details

                    foreach (QuestionnaireResponse response in participant.QuestionnaireResponses)
                    {
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(surveyItemSheetId)).Worksheet;
                        foreach (QuestionnaireResponseItem item in response.QuestionnaireResponseItems)
                        {
                            AddLineFromProperties(workingSheet, item, typeof(QuestionnaireResponseItem), surveyItemIndex,
                                                beforeCols: new List<string>() { participant.NHSNumber, response.Id.ToString() },
                                                afterCols: new List<string>() { item.Question.Text });
                            surveyItemIndex++;
                        }

                        workingSheet = ((WorksheetPart)wbPart.GetPartById(familyHistorySheetId)).Worksheet;
                        foreach (FamilyHistoryItem item in response.FamilyHistoryItems)
                        {
                            AddLineFromProperties(workingSheet, item, typeof(FamilyHistoryItem), familyHistoryIndex,
                                                beforeCols: new List<string>() { participant.NHSNumber, response.Id.ToString() });
                            familyHistoryIndex++;
                        }
                    }

                    // Add histology details

                    foreach (Histology hist in participant.Histologies)
                    {
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(histologySheetId)).Worksheet;
                        AddLineFromProperties(workingSheet, hist, typeof(Histology), histIndex,
                                            beforeCols: new List<string>() { participant.NHSNumber, participant.DateOfBirth.ToString(), participant.BMI.ToString(), participant.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString(), hist.Id.ToString() },
                                            afterCols: new List<string>() { hist.DiagnosisType.LookupDescription, hist.DiagnosisSide.LookupDescription });
                        histIndex++;

                        workingSheet = ((WorksheetPart)wbPart.GetPartById(histologyFocusSheetId)).Worksheet;
                        foreach (HistologyFocus item in hist.HistologyFoci)
                        {
                            AddLineFromProperties(workingSheet, item, typeof(HistologyFocus), histFocusIndex,
                                                beforeCols: new List<string>() { participant.NHSNumber, hist.Id.ToString() },
                                                afterCols: new List<string>() { item.Pathology.LookupDescription, item.Invasive.LookupDescription, item.DCISGrade.LookupDescription, item.VascularInvasion.LookupDescription, item.HER2Score.LookupDescription, item.TNMStage.LookupDescription });
                            histFocusIndex++;
                        }

                    }


                    parIndex++;
                }


                wbPart.Workbook.Save();
            }


            return generatedDocument;

        }

        /// <summary>
        /// Produce the histology report
        /// </summary>
        /// <returns>A stream containing the report</returns>
        public MemoryStream Histology()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add Histology header
                string histologySheetId = AddSheet(wbPart, "Histology", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(histologySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Histology), 1, beforeCols: new List<string>() { "NHSNumber", "DOB", "BMI", "RiskScore", "HistologyId" },
                                                                            afterCols: new List<string>() { "DiagnosisType", "DiagnosisSide" });

                // Add Histology focus header
                string histologyFocusSheetId = AddSheet(wbPart, "HistologyFocus",2);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(histologyFocusSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(HistologyFocus), 1, beforeCols: new List<string>() { "NHSNumber", "HistologyId" },
                                                                            afterCols: new List<string>() { "Pathology", "Invasive", "DCISGrade", "VascularInvasion", "HER2Score", "TNMStage" });

                int histIndex = 2;
                int histFocusIndex = 2;

                // Add histology details
                List<Histology> hists = _histologyRepo.GetAll().ToList();
                foreach (Histology hist in hists)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(histologySheetId)).Worksheet;
                    AddLineFromProperties(workingSheet, hist, typeof(Histology), histIndex,
                                        beforeCols: new List<string>() { hist.Participant.NHSNumber, hist.Participant.DateOfBirth.ToString(), hist.Participant.BMI.ToString(), hist.Participant.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString(), hist.Id.ToString() },
                                        afterCols: new List<string>() { hist.DiagnosisType.LookupDescription, hist.DiagnosisSide.LookupDescription });
                    histIndex++;

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(histologyFocusSheetId)).Worksheet;
                    foreach (HistologyFocus item in hist.HistologyFoci)
                    {
                        AddLineFromProperties(workingSheet, item, typeof(HistologyFocus), histFocusIndex,
                                            beforeCols: new List<string>() { hist.Participant.NHSNumber, hist.Id.ToString() },
                                            afterCols: new List<string>() { item.Pathology.LookupDescription, item.Invasive.LookupDescription, item.DCISGrade.LookupDescription, item.VascularInvasion.LookupDescription, item.HER2Score.LookupDescription, item.TNMStage.LookupDescription });
                        histFocusIndex++;
                    }

                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those invited but yet to consent.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream YetToConsent()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment" });
 
                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Where(x => x.Consented == false && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment" });
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }


        /// <summary>
        /// Produce a report of those consented but yet to have their full details uploaded.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream YetToGetFull()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Where(x => x.Consented == true && x.Deleted == false && x.LastName == null).ToList();
                foreach (Participant patient in patients)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who have Volpara data but no risk letter has been asked for yet.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream YetToAskForRisk()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.ScreeningRecordV1_5_4s).Where(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == false && x.ScreeningRecordV1_5_4s.Count > 0).ToList();
                foreach (Participant patient in patients)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }


        /// <summary>
        /// Produce a report of those who have asked for a letter but not received it yet.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream YetToReceiveLetter()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == true && x.SentRisk == false && x.RiskLetters.Count == 0).ToList();
                foreach (Participant patient in patients)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who have asked for a letter but not received it yet.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream YetToSendLetter()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.SentRisk == false && x.RiskLetters.Count > 0).ToList();
                foreach (Participant patient in patients)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who have consented but not received Volpara yet.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream WaitingForVolpara()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.ScreeningRecordV1_5_4s).Where(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == false && x.ScreeningRecordV1_5_4s.Count == 0).ToList();
                foreach (Participant patient in patients)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented" });
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

    }
}
