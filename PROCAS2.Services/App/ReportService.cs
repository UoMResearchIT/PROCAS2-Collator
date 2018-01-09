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

        private void AddHeaderFromProperties(Worksheet activeWorksheet, System.Type type, int rowindex)
        {
            Row headerRow = new Row();
            headerRow.RowIndex = (UInt32)rowindex;
            
            foreach (PropertyInfo pi in type.GetProperties())
            {
                if (pi.Name.EndsWith("Id") == false)
                {
                    if (pi.PropertyType == typeof(String) || pi.PropertyType == typeof(Int32) || pi.PropertyType == typeof(Boolean) ||
                        pi.PropertyType == typeof(int?) || pi.PropertyType == typeof(DateTime?) || pi.PropertyType == typeof(DateTime))
                    {
                        headerRow.AppendChild(AddCellWithText(pi.Name));
                    }
                }
            }

            activeWorksheet.Where(x => x.LocalName == "sheetData").First().AppendChild(headerRow);
        }


        private void AddLineFromProperties(Worksheet activeWorksheet, object data, System.Type type, int rowindex)
        {
            Row lineRow = new Row();
            lineRow.RowIndex = (UInt32)rowindex;


            foreach (PropertyInfo pi in type.GetProperties())
            {
                if (pi.Name.EndsWith("Id") == false)
                {
                    if (pi.PropertyType == typeof(String) || pi.PropertyType == typeof(Int32) || pi.PropertyType == typeof(Boolean) ||
                        pi.PropertyType == typeof(int?) || pi.PropertyType == typeof(DateTime?) || pi.PropertyType == typeof(DateTime))
                    {
                        lineRow.AppendChild(AddCellWithText(pi.GetValue(data) == null ? "" : pi.GetValue(data).ToString()));
                    }
                }
            }




            activeWorksheet.Where(x => x.LocalName == "sheetData").First().AppendChild(lineRow);
        }

        #endregion

        public MemoryStream PatientReport(List<string> NHSNumbers)
        {

            MemoryStream generatedDocument = new MemoryStream();

            using (SpreadsheetDocument spreadDoc = SpreadsheetDocument.Create(generatedDocument, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = AddWorkbookPart(spreadDoc);

                // Add participant header
                string mainSheetId = AddSheet(wbPart, "Main", 1);
                var workingSheet = ((WorksheetPart)wbPart.GetPartById(mainSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Participant), 1);

                // Add address header
                string addressSheetId = AddSheet(wbPart, "Addresses", 2);
                workingSheet = ((WorksheetPart)wbPart.GetPartById(addressSheetId)).Worksheet;
                AddHeaderFromProperties(workingSheet, typeof(Address), 1);

                int parIndex = 2;
                int addressIndex = 2;

                
                foreach (string NHSNumber in NHSNumbers.OrderBy(x => x)) 
                {
                    // Add participant details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(mainSheetId)).Worksheet;
                    Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).First();
                    AddLineFromProperties(workingSheet, participant, typeof(Participant), parIndex);

                    // Add address details
                    workingSheet = ((WorksheetPart)wbPart.GetPartById(addressSheetId)).Worksheet;
                    foreach (Address address in participant.Addresses)
                    {
                        AddLineFromProperties(workingSheet, address, typeof(Address), addressIndex);
                        addressIndex++;
                    }



                    parIndex++;
                }
               

                wbPart.Workbook.Save();
            }


            return generatedDocument;

        }

    
    }
}
