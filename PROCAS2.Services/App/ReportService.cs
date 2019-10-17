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

using CsvHelper;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Services.Utility;
using PROCAS2.Models.ViewModels.Reports;

namespace PROCAS2.Services.App
{
    public class ReportService : IReportService
    {

        private IGenericRepository<Participant> _participantRepo;
        private IGenericRepository<Histology> _histologyRepo;
        private IGenericRepository<VolparaDensity> _densityRepo;
        private IGenericRepository<QuestionnaireResponse> _responseRepo;
        private IConfigService _configService;

        private string racialBackgroundCode = "racialBackground"; // default - can be overridden in web.config

        public ReportService(IGenericRepository<Participant> participantRepo,
                IGenericRepository<Histology> histologyRepo,
                IGenericRepository<VolparaDensity> densityRepo,
                IGenericRepository<QuestionnaireResponse> responseRepo,
                IConfigService configService)
        {
            _participantRepo = participantRepo;
            _histologyRepo = histologyRepo;
            _densityRepo = densityRepo;
            _responseRepo = responseRepo;
            _configService = configService;

            racialBackgroundCode = _configService.GetAppSetting("RacialBackgroundCode");
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
        /// <param name="excludeCols">Columns to exclude (will have all if not null)</param>
        private void AddHeaderFromProperties(Worksheet activeWorksheet, System.Type type, int rowindex,
                                            List<string> beforeCols = null,
                                            List<string> afterCols = null,
                                            List<string> onlyCols = null,
                                            List<string> excludeCols = null)
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
                if (excludeCols == null || (excludeCols != null && !excludeCols.Contains(pi.Name)))
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
        /// <param name="excludeCols">Columns to exclude (will have all if not null)</param>
        private void AddLineFromProperties(Worksheet activeWorksheet, object data, System.Type type, int rowindex,
                                            List<string> beforeCols = null,
                                            List<string> afterCols = null,
                                            List<string> onlyCols = null,
                                            List<string> excludeCols = null)
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
                if (excludeCols == null || (excludeCols != null && !excludeCols.Contains(pi.Name)))
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
        /// Query the database using the passed parameters
        /// </summary>
        /// <param name="model">View model containing the parameters</param>
        /// <returns>MemoryStream of generated document</returns>
        public MemoryStream QueryDB(QueryDatabaseViewModel model)
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);

                // Add participant header
                string mainSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(mainSheetId)).Worksheet;
                List<string> exCols = new List<string>() { "HashedNHSNumber" };
                if (model.Anonymise == true)
                {
                    exCols.Add("NHSNumber");
                    exCols.Add("FirstName");
                    exCols.Add("LastName");
                    exCols.Add("Title");
                    exCols.Add("GPName");
                }

                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, 
                    afterCols: new List<string>() { "Site", "ChemoDetails", "InitialScreeningOutcome", "FinalAssessmentOutcome", "FinalTechnicalOutcome", "RiskConsultationType" },
                    beforeCols: new List<string>() { "StudyNumber"},
                    excludeCols: exCols);

                string addressSheetId = "";
                if (model.Anonymise == false && model.IncludeAddresses == true)
                {
                    // Add address header
                    addressSheetId = AddSheet(wbPart, "Addresses", 2);
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(addressSheetId)).Worksheet;
                    AddHeaderFromProperties(workingSheet, typeof(Address), 1,
                                                beforeCols: new List<string>() { "StudyNumber" },
                                                afterCols: new List<string>() { "Type" });
                }

                // Add Volpara header
                string volparaSheetId = AddSheet(wbPart, "Volpara", 3);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(volparaSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(ScreeningRecordV1_5_4), 1, beforeCols: new List<string>() { "StudyNumber" });

                // Add Volpara density header
                string densitySheetId = AddSheet(wbPart, "Density", 4);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(densitySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(VolparaDensity), 1, beforeCols: new List<string>() { "StudyNumber" });

                // Add Risk letter header
                string riskSheetId = AddSheet(wbPart, "Risk", 5);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(riskSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(RiskLetter), 1, beforeCols: new List<string>() { "StudyNumber" });

                // Add Survey header
                string surveySheetId = AddSheet(wbPart, "SurveyHeader", 6);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(surveySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(QuestionnaireResponse), 1, beforeCols: new List<string>() { "StudyNumber", "ResponseId" });

                // Add Survey item header
                string surveyItemSheetId = AddSheet(wbPart, "SurveyItems", 7);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(surveyItemSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(QuestionnaireResponseItem), 1, beforeCols: new List<string>() { "StudyNumber", "ResponseId" },
                                                                                            afterCols: new List<string>() { "QuestionText" });

                // Add family history header
                string familyHistorySheetId = AddSheet(wbPart, "FamilyHistory", 8);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(familyHistorySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(FamilyHistoryItem), 1, beforeCols: new List<string>() { "StudyNumber", "ResponseId" });

                // Add family genetic testing header
                string familyGeneticSheetId = AddSheet(wbPart, "FamilyGenetic", 9);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(familyGeneticSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(FamilyGeneticTestingItem), 1, beforeCols: new List<string>() { "StudyNumber", "ResponseId" });

                string histologySheetId = "";
                string histologyFocusSheetId = "";

                if (model.IncludeHistology == true)
                {
                    // Add Histology header
                    histologySheetId = AddSheet(wbPart, "Histology", 10);
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(histologySheetId)).Worksheet;
                    AddHeaderFromProperties(workingSheet, typeof(Histology), 1, beforeCols: new List<string>() { "StudyNumber", "DOB", "BMI", "RiskScore", "HistologyId" },
                                                                                afterCols: new List<string>() { "DiagnosisType", "DiagnosisSide" });

                    // Add Histology focus header
                    histologyFocusSheetId = AddSheet(wbPart, "HistologyFocus", 11);
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(histologyFocusSheetId)).Worksheet;
                    AddHeaderFromProperties(workingSheet, typeof(HistologyFocus), 1, beforeCols: new List<string>() { "StudyNumber", "HistologyId" },
                                                                                afterCols: new List<string>() { "InvasiveTumourType", "InSituTumourType", "Invasive", "DCISGrade", "VascularInvasion", "HER2Score", "TNMStageT", "TNMStageN" });
                }
                int parIndex = 2;
                int addressIndex = 2;
                int volparaIndex = 2;
                int densityIndex = 2;
                int riskIndex = 2;
                int surveyIndex = 2;
                int surveyItemIndex = 2;
                int familyHistoryIndex = 2;
                int familyGeneticIndex = 2;
                int histIndex = 2;
                int histFocusIndex = 2;

                DateTime dateFrom = model.ConsentedFrom.AddDays(-1);
                DateTime dateTo = model.ConsentedTo.AddDays(1);
                List<Participant> participants = _participantRepo.GetAll().Where(x => x.Consented == true && x.Deleted == false && x.DateConsented >= dateFrom && x.DateConsented <= dateTo && x.ScreeningSite.Code == model.ScreeningSite ).ToList();

                foreach (Participant participant in participants)
                {
                    // Add participant details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(mainSheetId)).Worksheet;
                    exCols = new List<string>() { "HashedNHSNumber" };
                    if (model.Anonymise == true)
                    {
                        exCols.Add("NHSNumber");
                        exCols.Add("FirstName");
                        exCols.Add("LastName");
                        exCols.Add("Title");
                        exCols.Add("GPName");
                    }

                    AddLineFromProperties(workingSheet, participant, typeof(Participant), parIndex, 
                        afterCols: new List<string>() { participant.ScreeningSite.Name,
                                   participant.ChemoPreventionDetails==null?null:participant.ChemoPreventionDetails.LookupDescription,
                                   participant.InitialScreeningOutcome==null?null:participant.InitialScreeningOutcome.LookupDescription,
                                   participant.FinalAssessmentOutcome==null?null:participant.FinalAssessmentOutcome.LookupDescription,
                                   participant.FinalTechnicalOutcome == null?null:participant.FinalTechnicalOutcome.LookupDescription,
                                   participant.RiskConsultationType == null?null:participant.RiskConsultationType.LookupDescription
                                    },
                        beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0' )},
                        excludeCols: exCols
                        );

                    // Add address details
                    if (model.Anonymise == false && model.IncludeAddresses == true)
                    {
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(addressSheetId)).Worksheet;
                        foreach (Address address in participant.Addresses)
                        {
                            AddLineFromProperties(workingSheet, address, typeof(Address), addressIndex,
                                                beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0') },
                                                afterCols: new List<string>() { address.AddressType.Name });
                            addressIndex++;
                        }
                    }

                    
                    foreach (VolparaDensity density in participant.VolparaDensities.OrderByDescending(x => x.DataDate))
                    {
                        // Add Volpara density details
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(densitySheetId)).Worksheet;
                        AddLineFromProperties(workingSheet, density, typeof(VolparaDensity), densityIndex,
                                            beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0') });
                        densityIndex++;

                        // Add Volpara details
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(volparaSheetId)).Worksheet;
                        foreach (ScreeningRecordV1_5_4 screening in density.Participant.ScreeningRecordV1_5_4s.Where(x => x.VolparaDensity != null && x.VolparaDensity.Id == density.Id))
                        {
                            AddLineFromProperties(workingSheet, screening, typeof(ScreeningRecordV1_5_4), volparaIndex,
                                                beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0') });
                            volparaIndex++;
                        }

                        if (model.OnlyMostRecentVolpara == true)
                        {
                            break;
                        }
                    }
 

                    // Add Risk Letter details
                    foreach (RiskLetter letter in participant.RiskLetters.OrderByDescending(x => x.DateReceived))
                    {
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(riskSheetId)).Worksheet;
                        AddLineFromProperties(workingSheet, letter, typeof(RiskLetter), riskIndex,
                                            beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0') },
                                            excludeCols: new List<string>() { "RiskLetterContent" });
                        riskIndex++;

                        if (model.OnlyMostRecentRisk == true)
                        {
                            break;
                        }
                    }


                    
                   

                    // Add survey item and famnily history details

                    foreach (QuestionnaireResponse response in participant.QuestionnaireResponses.OrderByDescending(x => x.DateReceived))
                    {
                        // Add survey header details
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(surveySheetId)).Worksheet;
                        AddLineFromProperties(workingSheet, response, typeof(QuestionnaireResponse), surveyIndex,
                                            beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0'), response.Id.ToString() });
                        surveyIndex++;

                        workingSheet = ((WorksheetPart)wbPart.GetPartById(surveyItemSheetId)).Worksheet;
                        foreach (QuestionnaireResponseItem item in response.QuestionnaireResponseItems.OrderBy(x => x.Question.QuestionNum))
                        {
                            AddLineFromProperties(workingSheet, item, typeof(QuestionnaireResponseItem), surveyItemIndex,
                                                beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0'), response.Id.ToString() },
                                                afterCols: new List<string>() { item.Question.Text });
                            surveyItemIndex++;
                        }

                        workingSheet = ((WorksheetPart)wbPart.GetPartById(familyHistorySheetId)).Worksheet;
                        foreach (FamilyHistoryItem item in response.FamilyHistoryItems)
                        {
                            AddLineFromProperties(workingSheet, item, typeof(FamilyHistoryItem), familyHistoryIndex,
                                                beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0'), response.Id.ToString() });
                            familyHistoryIndex++;
                        }

                        workingSheet = ((WorksheetPart)wbPart.GetPartById(familyGeneticSheetId)).Worksheet;
                        foreach (FamilyGeneticTestingItem item in response.FamilyGeneticTestingItems)
                        {
                            AddLineFromProperties(workingSheet, item, typeof(FamilyGeneticTestingItem), familyGeneticIndex,
                                                beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0'), response.Id.ToString() });
                            familyGeneticIndex++;
                        }

                        if (model.OnlyMostRecentQuestionnaire == true)
                        {
                            break;
                        }
                    }

                    // Add histology details
                    if (model.IncludeHistology == true)
                    {
                        foreach (Histology hist in participant.Histologies)
                        {
                            workingSheet = ((WorksheetPart)wbPart.GetPartById(histologySheetId)).Worksheet;
                            AddLineFromProperties(workingSheet, hist, typeof(Histology), histIndex,
                                                beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0'), participant.DateOfBirth.ToString(), participant.BMI.ToString(), participant.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString(), hist.Id.ToString() },
                                                afterCols: new List<string>() { hist.DiagnosisType.LookupDescription, hist.DiagnosisSide.LookupDescription });
                            histIndex++;

                            workingSheet = ((WorksheetPart)wbPart.GetPartById(histologyFocusSheetId)).Worksheet;
                            foreach (HistologyFocus item in hist.HistologyFoci)
                            {
                                AddLineFromProperties(workingSheet, item, typeof(HistologyFocus), histFocusIndex,
                                                    beforeCols: new List<string>() { participant.StudyNumber.ToString().PadLeft(5, '0'), hist.Id.ToString() },
                                                    afterCols: new List<string>() { item.InvasiveTumourType.LookupDescription, item.InSituTumourType.LookupDescription, item.Invasive.LookupDescription, item.DCISGrade.LookupDescription, item.VascularInvasion.LookupDescription, item.HER2Score.LookupDescription, item.TNMStageT.LookupDescription, item.TNMStageN.LookupDescription });
                                histFocusIndex++;
                            }

                        }
                    }


                    parIndex++;
                }


                wbPart.Workbook.Save();
            }



            return generatedDocument;
        }

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
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, afterCols: new List<string>() { "Site", "ChemoDetails", "InitialScreeningOutcome", "FinalAssessmentOutcome", "FinalTechnicalOutcome", "RiskConsultationType" });

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

                // Add Volpara density header
                string densitySheetId = AddSheet(wbPart, "Density", 4);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(densitySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(VolparaDensity), 1, beforeCols: new List<string>() { "NHSNumber" });

                // Add Risk letter header
                string riskSheetId = AddSheet(wbPart, "Risk", 5);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(riskSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(RiskLetter), 1, beforeCols: new List<string>() { "NHSNumber" });

                // Add Survey header
                string surveySheetId = AddSheet(wbPart, "SurveyHeader", 6);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(surveySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(QuestionnaireResponse), 1, beforeCols: new List<string>() { "NHSNumber", "ResponseId" });

                // Add Survey item header
                string surveyItemSheetId = AddSheet(wbPart, "SurveyItems", 7);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(surveyItemSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(QuestionnaireResponseItem), 1, beforeCols: new List<string>() { "NHSNumber", "ResponseId" },
                                                                                            afterCols: new List<string>() { "QuestionText" });

                // Add family history header
                string familyHistorySheetId = AddSheet(wbPart, "FamilyHistory", 8);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(familyHistorySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(FamilyHistoryItem), 1, beforeCols: new List<string>() { "NHSNumber", "ResponseId" });

                // Add family genetic testing header
                string familyGeneticSheetId = AddSheet(wbPart, "FamilyGenetic", 9);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(familyGeneticSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(FamilyGeneticTestingItem), 1, beforeCols: new List<string>() { "NHSNumber", "ResponseId" });


                // Add Histology header
                string histologySheetId = AddSheet(wbPart, "Histology",10 );
                workingSheet = ((WorksheetPart)wbPart.GetPartById(histologySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Histology), 1, beforeCols: new List<string>() { "NHSNumber", "DOB", "BMI", "RiskScore", "HistologyId" },
                                                                            afterCols: new List<string>() { "DiagnosisType", "DiagnosisSide" });

                // Add Histology focus header
                string histologyFocusSheetId = AddSheet(wbPart, "HistologyFocus", 11);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(histologyFocusSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(HistologyFocus), 1, beforeCols: new List<string>() { "NHSNumber", "HistologyId" },
                                                                            afterCols: new List<string>() { "InvasiveTumourType", "InSituTumourType", "Invasive", "DCISGrade", "VascularInvasion", "HER2Score", "TNMStageT", "TNMStageN" });

                int parIndex = 2;
                int addressIndex = 2;
                int volparaIndex = 2;
                int densityIndex = 2;
                int riskIndex = 2;
                int surveyIndex = 2;
                int surveyItemIndex = 2;
                int familyHistoryIndex = 2;
                int familyGeneticIndex = 2;
                int histIndex = 2;
                int histFocusIndex = 2;


                foreach (string NHSNumber in NHSNumbers.OrderBy(x => x))
                {
                    // Add participant details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(mainSheetId)).Worksheet;
                    Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber  && x.Consented == true && x.Deleted == false).FirstOrDefault();
                    if (participant == null)
                    {
                        // Don't try to continue the report for this participant if we can't find them or they haven't consented yet.
                        continue;
                    }

                    AddLineFromProperties(workingSheet, participant, typeof(Participant), parIndex, afterCols: new List<string>() { participant.ScreeningSite.Name,
                                                                                                                    participant.ChemoPreventionDetails==null?null:participant.ChemoPreventionDetails.LookupDescription,
                                                                                                                    participant.InitialScreeningOutcome==null?null:participant.InitialScreeningOutcome.LookupDescription,
                                                                                                                    participant.FinalAssessmentOutcome==null?null:participant.FinalAssessmentOutcome.LookupDescription,
                                                                                                                    participant.FinalTechnicalOutcome == null?null:participant.FinalTechnicalOutcome.LookupDescription,
                                                                                                                    participant.RiskConsultationType == null?null:participant.RiskConsultationType.LookupDescription                                                    
                    });

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

                    // Add Volpara density details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(densitySheetId)).Worksheet;
                    foreach (VolparaDensity density in participant.VolparaDensities)
                    {
                        AddLineFromProperties(workingSheet, density, typeof(VolparaDensity), densityIndex,
                                            beforeCols: new List<string>() { participant.NHSNumber });
                        densityIndex++;
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
                        foreach (QuestionnaireResponseItem item in response.QuestionnaireResponseItems.OrderBy(x => x.Question.QuestionNum))
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

                        workingSheet = ((WorksheetPart)wbPart.GetPartById(familyGeneticSheetId)).Worksheet;
                        foreach (FamilyGeneticTestingItem item in response.FamilyGeneticTestingItems)
                        {
                            AddLineFromProperties(workingSheet, item, typeof(FamilyGeneticTestingItem), familyGeneticIndex,
                                                beforeCols: new List<string>() { participant.NHSNumber, response.Id.ToString() });
                            familyGeneticIndex++;
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
                                                afterCols: new List<string>() { item.InvasiveTumourType.LookupDescription, item.InSituTumourType.LookupDescription, item.Invasive.LookupDescription, item.DCISGrade.LookupDescription, item.VascularInvasion.LookupDescription, item.HER2Score.LookupDescription, item.TNMStageT.LookupDescription, item.TNMStageN.LookupDescription });
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
        /// Produce the Volpara report
        /// </summary>
        /// <returns>A stream containing the report</returns>
        public MemoryStream Volpara()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add Density header
                string densitySheetId = AddSheet(wbPart, "Density", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(densitySheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(VolparaDensity), 1, beforeCols: new List<string>() { "NHSNumber", "DensityId" });

                // Add Screening record header
                string screeningRecordSheetId = AddSheet(wbPart, "ScreeningRecord", 2);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(screeningRecordSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(ScreeningRecordV1_5_4), 1, beforeCols: new List<string>() { "NHSNumber", "DensityId" });

                int densityIndex = 2;
                int screeningRecordIndex = 2;

                // Add Volpara details
                List<VolparaDensity> densities = _densityRepo.GetAll().OrderByDescending(x => x.DataDate).ToList();
                foreach (VolparaDensity density in densities)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(densitySheetId)).Worksheet;
                    AddLineFromProperties(workingSheet, density, typeof(VolparaDensity), densityIndex,
                                        beforeCols: new List<string>() { density.Participant.NHSNumber, density.Id.ToString() }
                                        );
                    densityIndex++;

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(screeningRecordSheetId)).Worksheet;
                    foreach (ScreeningRecordV1_5_4 item in density.Participant.ScreeningRecordV1_5_4s.Where(x => x.VolparaDensityId == density.Id).OrderByDescending(x => x.DataDate))
                    {
                        AddLineFromProperties(workingSheet, item, typeof(ScreeningRecordV1_5_4), screeningRecordIndex,
                                            beforeCols: new List<string>() { item.Participant.NHSNumber, density.Id.ToString() });
                        screeningRecordIndex++;
                    }

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
                AddHeaderFromProperties(workingSheet, typeof(Histology), 1, beforeCols: new List<string>() { "NHSNumber", "DOB", "RiskScore" },
                                                                            onlyCols: new List<string>() { "MammogramDate", "DiagnosisDate" },
                                                                            afterCols: new List<string>() { "DiagnosisSide" });
       
                int histIndex = 2;

                // Add histology details
                List<Histology> hists = _histologyRepo.GetAll().ToList();
                foreach (Histology hist in hists)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(histologySheetId)).Worksheet;
                    AddLineFromProperties(workingSheet, hist, typeof(Histology), histIndex,
                                        beforeCols: new List<string>() { hist.Participant.NHSNumber, hist.Participant.DateOfBirth.ToString(),  hist.Participant.RiskLetters.Count == 0? "Not found": hist.Participant.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString() },
                                        onlyCols: new List<string>() { "MammogramDate", "DiagnosisDate" },
                                        afterCols: new List<string>() {  hist.DiagnosisSide.LookupDescription });
                    histIndex++;    
                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of all those who have been invited.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream Invited()
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
                DateTime invitedSince = DateTime.Now.AddMonths(-2);
                List<Participant> patients = _participantRepo.GetAll().Where(x => x.Deleted == false && x.DateCreated.HasValue && x.DateCreated > invitedSince).ToList();
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
        /// Produce a report of all those who have consented.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream Consented()
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
                List<Participant> patients = _participantRepo.GetAll().Where(x => x.Consented == true && x.Deleted == false).ToList();
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
                List<Participant> patients = _participantRepo.GetAll().Where(x => x.Consented == true && x.Deleted == false && x.LastName == null && x.Deleted == false).ToList();
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
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.ScreeningRecordV1_5_4s).Where(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == false && x.ScreeningRecordV1_5_4s.Count > 0 && x.Deleted == false).ToList();
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
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == true && x.SentRisk == false && x.RiskLetters.Count == 0 && x.Deleted == false).ToList();
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
        /// Produce a report containing those participants who have been flagged as requiring a risk letter but haven't receivied one yet
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream AskForRiskLetters()
        {
            MemoryStream generatedDocument = new MemoryStream();           

            StringWriter csvString = new StringWriter();

            using (var csv = new CsvWriter(csvString))
            {

                csv.Configuration.Delimiter = "|";


                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == true && x.SentRisk == false && x.RiskLetters.Count == 0 && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {
                    // First the study number
                    csv.WriteField(patient.StudyNumber.ToString().PadLeft(5, '0'));

                    // Then DOB
                    csv.WriteField(patient.DateOfBirth.HasValue? patient.DateOfBirth.Value.ToString("yyyyMMdd"): "Unknown");

                    // Then date of first appointment
                    csv.WriteField(patient.DateFirstAppointment.HasValue ? patient.DateFirstAppointment.Value.ToString("yyyyMMdd") : "Unknown");

                    // Then the hash
                    csv.WriteField(patient.HashedNHSNumber);

                    // Then a record indicating to release the results
                    csv.WriteField("ReleaseResults");

                    csv.NextRecord();
                    
                }
            }

            generatedDocument = new MemoryStream(Encoding.UTF8.GetBytes(csvString.ToString()));

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
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.SentRisk == false && x.RiskLetters.Count > 0 && x.Deleted == false).ToList();
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
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.ScreeningRecordV1_5_4s).Where(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == false && x.ScreeningRecordV1_5_4s.Count == 0 && x.Deleted == false).ToList();
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
        /// Produce a report of those who have consented but not received Volpara yet, and are within 2 weeks of 6 week deadline
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream WaitingForVolparaNear6Weeks()
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

                int noVolparaWarningDays = _configService.GetIntAppSetting("NoVolparaWarningDays") ?? 0;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.ScreeningRecordV1_5_4s).Where(x => x.Consented == true && x.LastName != null && x.AskForRiskLetter == false && x.ScreeningRecordV1_5_4s.Count == 0 && x.Deleted == false && x.DateFirstAppointment.HasValue && DbFunctions.DiffDays(x.DateFirstAppointment.Value, DateTime.Now) >= noVolparaWarningDays).ToList();
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
        /// Produce a report of those who have said that they've had nipple piercings
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream NipplePiercings()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented", "SentRisk" },
                                                                              afterCols: new List<string>() {"Implants", "ImplantsSide", "Reduction", "ReductionSide", "Piercings", "PiercingsSide", "Other", "OtherSide" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.QuestionnaireResponses).
                                                                            Where(x => x.Consented == true && 
                                                                            x.LastName != null && 
                                                                            x.QuestionnaireResponses.Count > 0 && 
                                                                            x.QuestionnaireResponses.OrderByDescending(b => b.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(c => c.Question.Code == "OtherBreastSurgeryImplantsReductionPiercing" && c.ResponseText == "Yes").Count() > 0 &&
                                                                            x.Deleted == false && 
                                                                            x.DateFirstAppointment.HasValue)
                                                                       .OrderByDescending(x => x.DateFirstAppointment).ToList();
                foreach (Participant patient in patients)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    QuestionnaireResponseItem implants = patient.QuestionnaireResponses.OrderByDescending(b => b.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(c => c.Question.Code.ToLower() == "breastImplants".ToLower()).FirstOrDefault();
                    QuestionnaireResponseItem implantsSide = patient.QuestionnaireResponses.OrderByDescending(b => b.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(c => c.Question.Code.ToLower() == "breastImplantsSide".ToLower()).FirstOrDefault();
                    QuestionnaireResponseItem reduction = patient.QuestionnaireResponses.OrderByDescending(b => b.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(c => c.Question.Code.ToLower() == "breastReduction".ToLower()).FirstOrDefault();
                    QuestionnaireResponseItem reductionSide = patient.QuestionnaireResponses.OrderByDescending(b => b.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(c => c.Question.Code.ToLower() == "breastReductionSide".ToLower()).FirstOrDefault();
                    QuestionnaireResponseItem nipplePiercing = patient.QuestionnaireResponses.OrderByDescending(b => b.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(c => c.Question.Code.ToLower() == "nipplePiercing".ToLower()).FirstOrDefault();
                    QuestionnaireResponseItem nipplePiercingSide = patient.QuestionnaireResponses.OrderByDescending(b => b.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(c => c.Question.Code.ToLower() == "nipplePiercingSide".ToLower()).FirstOrDefault();
                    QuestionnaireResponseItem other = patient.QuestionnaireResponses.OrderByDescending(b => b.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(c => c.Question.Code.ToLower() == "OtherBreastSurgeryYesNo".ToLower()).FirstOrDefault();
                    QuestionnaireResponseItem otherSide = patient.QuestionnaireResponses.OrderByDescending(b => b.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(c => c.Question.Code.ToLower() == "OtherBreastSurgerySide".ToLower()).FirstOrDefault();

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "DateCreated", "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateConsented", "SentRisk" },
                                        afterCols: new List<string>()
                                        {
                                            implants == null? "": implants.ResponseText,
                                            implantsSide == null? "": implantsSide.ResponseText,
                                            reduction == null? "": reduction.ResponseText,
                                            reductionSide == null? "": reductionSide.ResponseText,
                                            nipplePiercing == null? "": nipplePiercing.ResponseText,
                                            nipplePiercingSide == null? "": nipplePiercingSide.ResponseText,
                                            other == null? "": other.ResponseText,
                                            otherSide == null? "": otherSide.ResponseText,
                                        });
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }


        /// <summary>
        /// Produce a report of those who attended screening on the day of the first offered appointment.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream ScreeningFirstOffered()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 &&  x.DateActualAppointment == x.DateFirstAppointment && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who attended screening within 180 days of the first offered appointment.
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream ScreeningWithin180Days()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.DateActualAppointment >= x.DateFirstAppointment && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {
                    // check within certain number of days.
                    if ((patient.DateActualAppointment.Value - patient.DateFirstAppointment.Value).TotalDays <= 180)
                    {
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                        string ethnicity = "";
                        if (patient.QuestionnaireResponses.Count > 0)
                        {
                            if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                            {
                                ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                            }
                        }

                        AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                            onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                            afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                        repIndex++;
                    }

                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who had technical recalls
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream NumberTechnicalRecalls()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.InitialScreeningOutcome != null && x.InitialScreeningOutcome.LookupCode == "INI_TECH" && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {
                   
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }


                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                            onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                            afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                       ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                        repIndex++;
                   

                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who had assessment recalls
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream NumberAssessmentRecalls()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.InitialScreeningOutcome != null && x.InitialScreeningOutcome.LookupCode == "INI_ASSESS" && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who had routine recalls
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream NumberRoutineRecalls()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.InitialScreeningOutcome != null && x.InitialScreeningOutcome.LookupCode == "INI_ROUTINE" && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }


        /// <summary>
        /// Produce a report of those who had declined mammogram
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream DeclinedMammogram()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.InitialScreeningOutcome != null && x.InitialScreeningOutcome.LookupCode == "INI_DECLINE" && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those patients for who we have possibly received an unfinished questionnaire
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream UnfinishedQuestionnaire()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "DateReceived" },
                                                                               beforeCols: new List<string>() {"NHSNumber" });

                int repIndex = 2;

                // Add details
                List<QuestionnaireResponse> responses = _responseRepo.GetAll().Where(x => x.QuestionnaireEnd == null).ToList();
                foreach (QuestionnaireResponse response in responses.OrderByDescending(x => x.DateReceived))
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;


                    AddLineFromProperties(workingSheet, response, typeof(QuestionnaireResponse), repIndex,
                                        onlyCols: new List<string>() { "DateReceived"},
                                        beforeCols: new List<string>() {response.Participant.NHSNumber});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }


        /// <summary>
        /// Produce a report of those who had chemo appointment but treatment was disagreed in clinic 
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream ChemoDisagreed()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true  && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.Chemoprevention == true && x.ChemoAgreedInClinic == false && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }


        /// <summary>
        /// Produce a report of those who had chemo appointment but treatment was agreed in clinic but not considered appropriate 
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream ChemoNotApp()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.Chemoprevention == true && x.ChemoAgreedInClinic == true && x.ChemoPreventionDetails != null && x.ChemoPreventionDetails.LookupCode == "CHEMONA" && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who had chemo appointment but treatment was agreed in clinic but the prescription not filled in 
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream ChemoNotFilled()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.Chemoprevention == true && x.ChemoAgreedInClinic == true && x.ChemoPreventionDetails != null && x.ChemoPreventionDetails.LookupCode == "CHEMOAPPNOT" && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }


        /// <summary>
        /// Produce a report of those who had chemo appointment but treatment was agreed in clinic and the prescription filled in 
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream ChemoFilled()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.Chemoprevention == true && x.ChemoAgreedInClinic == true && x.ChemoPreventionDetails != null && x.ChemoPreventionDetails.LookupCode == "CHEMOAPPFILL" && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who have subsequently attended family history services
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream SubsequentFamilyHistory()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

                int repIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.FHCReferral == true && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }

        /// <summary>
        /// Produce a report of those who have subsequently attended more frequent screening
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream SubsequentMoreFrequent()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);



                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });


                int repIndex = 2;
 

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.MoreFrequentScreening == true && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }


        /// <summary>
        /// Produce a report of those who have been diagnosed with breast cancer
        /// </summary>
        /// <returns>The report!</returns>
        public MemoryStream BreastCancerDiagnoses()
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);


                // Add header
                string repSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment", "BMI" },
                                                                                afterCols: new List<string>() { "AgeAtConsent", "Ethnicity", "PostCode", "Risk" });

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


                int repIndex = 2;
                int surveyIndex = 2;
                int surveyItemIndex = 2;
                int familyHistoryIndex = 2;

                // Add details
                List<Participant> patients = _participantRepo.GetAll().Include(a => a.RiskLetters).Where(x => x.Consented == true && x.LastName != null && x.DateFirstAppointment != null && x.RiskLetters.Count > 0 && x.Diagnosed == true && x.Deleted == false).ToList();
                foreach (Participant patient in patients)
                {

                    workingSheet = ((WorksheetPart)wbPart.GetPartById(repSheetId)).Worksheet;

                    string ethnicity = "";
                    if (patient.QuestionnaireResponses.Count > 0)
                    {
                        if (patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).Count() > 0)
                        {
                            ethnicity = patient.QuestionnaireResponses.OrderByDescending(x => x.DateReceived).FirstOrDefault().QuestionnaireResponseItems.Where(x => x.Question.Code == racialBackgroundCode).FirstOrDefault().ResponseText;
                        }
                    }

                    AddLineFromProperties(workingSheet, patient, typeof(Participant), repIndex,
                                        onlyCols: new List<string>() { "NHSNumber", "DateOfBirth", "DateFirstAppointment", "DateActualAppointment", "BMI" },
                                        afterCols: new List<string>() { WholeYearsDiff(patient.DateConsented.Value, patient.DateOfBirth.Value),
                                                                        ethnicity,
                                                                        patient.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault().PostCode,
                                                                        patient.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault().RiskScore.ToString()});
                    
                    // Add survey header details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(surveySheetId)).Worksheet;
                    foreach (QuestionnaireResponse response in patient.QuestionnaireResponses)
                    {
                        AddLineFromProperties(workingSheet, response, typeof(QuestionnaireResponse), surveyIndex,
                                            beforeCols: new List<string>() { patient.NHSNumber, response.Id.ToString() });
                        surveyIndex++;
                    }

                    // Add survey item and famnily history details

                    foreach (QuestionnaireResponse response in patient.QuestionnaireResponses)
                    {
                        workingSheet = ((WorksheetPart)wbPart.GetPartById(surveyItemSheetId)).Worksheet;
                        foreach (QuestionnaireResponseItem item in response.QuestionnaireResponseItems)
                        {
                            AddLineFromProperties(workingSheet, item, typeof(QuestionnaireResponseItem), surveyItemIndex,
                                                beforeCols: new List<string>() { patient.NHSNumber, response.Id.ToString() },
                                                afterCols: new List<string>() { item.Question.Text });
                            surveyItemIndex++;
                        }

                        workingSheet = ((WorksheetPart)wbPart.GetPartById(familyHistorySheetId)).Worksheet;
                        foreach (FamilyHistoryItem item in response.FamilyHistoryItems)
                        {
                            AddLineFromProperties(workingSheet, item, typeof(FamilyHistoryItem), familyHistoryIndex,
                                                beforeCols: new List<string>() { patient.NHSNumber, response.Id.ToString() });
                            familyHistoryIndex++;
                        }
                    }

                    repIndex++;


                }

                wbPart.Workbook.Save();
            }

            return generatedDocument;
        }


        /// <summary>
        /// Calculate the number of whole years between 2 dates
        /// </summary>
        /// <param name="laterDate">later date</param>
        /// <param name="earlierDate">earlier date</param>
        /// <returns>number of years</returns>
        private string WholeYearsDiff(DateTime laterDate, DateTime earlierDate)
        {
            if(laterDate.DayOfYear >= earlierDate.DayOfYear)
            {
                return (laterDate.Year - earlierDate.Year).ToString();
            }
            else
            {
                return (laterDate.Year - earlierDate.Year - 1 ).ToString();
            }
        }

    }
}
