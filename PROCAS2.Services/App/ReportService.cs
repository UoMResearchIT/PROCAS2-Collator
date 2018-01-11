using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using PROCAS2.Data;
using PROCAS2.Data.Entities;


namespace PROCAS2.Services.App
{
    public class ReportService:IReportService
    {

        private IGenericRepository<Participant> _participantRepo;

        public ReportService(IGenericRepository<Participant> participantRepo)
        {
            _participantRepo = participantRepo;
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
        private string AddSheet (WorkbookPart wbPart, string sheetName, uint sheetId)
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

            if(afterCols != null)
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
                foreach(string col in beforeCols)
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
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1, afterCols:new List<string>() { "Site" });

                // Add address header
                string addressSheetId = AddSheet(wbPart, "Addresses", 2);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(addressSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Address), 1, 
                                            beforeCols:new List<string>() {"NHSNumber" }, 
                                            afterCols:new List<string>() { "Type" });

                // Add Volpara header
                string volparaSheetId = AddSheet(wbPart, "Volpara", 3);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(volparaSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(ScreeningRecordV1_5_4), 1, beforeCols: new List<string>() { "NHSNumber" });

                // Add Risk letter header
                string riskSheetId = AddSheet(wbPart, "Risk", 4);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(riskSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(RiskLetter), 1, beforeCols: new List<string>() { "NHSNumber" });

                int parIndex = 2;
                int addressIndex = 2;
                int volparaIndex = 2;
                int riskIndex = 2;

                
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
                                            afterCols: new List<string>() { address.AddressType.Name});
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



                    parIndex++;
                }
               

                wbPart.Workbook.Save();
            }


            return generatedDocument;

        }

    
    }
}
