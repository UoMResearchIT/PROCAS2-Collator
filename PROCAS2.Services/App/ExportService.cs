using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.IO;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NotesFor.HtmlToOpenXml;

using PROCAS2.Models.ViewModels;
using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Resources;

namespace PROCAS2.Services.App
{
    public class ExportService : IExportService
    {
        private IUnitOfWork _unitOfWork;
        private IGenericRepository<Participant> _participantRepo;
        private IGenericRepository<RiskLetter> _riskLetterRepo;

        public ExportService(IGenericRepository<Participant> participantRepo,
                            IGenericRepository<RiskLetter> riskLetterRepo,
                            IUnitOfWork unitOfWork)
        {
            _participantRepo = participantRepo;
            _riskLetterRepo = riskLetterRepo;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Validate the NHS number and make sure it is ready for exporting.
        /// </summary>
        /// <param name="NHSNumber">NHS number</param>
        /// <param name="errString">returned error message</param>
        /// <returns>true if valid, else false</returns>
        public bool ValidateNHSNumberForExport(string NHSNumber, out string errString)
        {
            errString = "";

            Participant participant = _participantRepo.GetAll().Where(x => x.NHSNumber == NHSNumber).FirstOrDefault();
            if (participant != null)
            {
                if (participant.Consented == false)
                {
                    errString = ExportResources.NOT_CONSENTED;
                    return false;
                }

                if (participant.RiskLetters.Count == 0)
                {
                    errString = ExportResources.NO_LETTER_YET;
                    return false;
                }

                if (String.IsNullOrEmpty(participant.FirstName) == true)
                {
                    errString = ExportResources.NO_DETAILS_YET;
                    return false;
                }

            }
            else
            {
                errString = ExportResources.NHS_NUMBER_INVALID;
                return false;
            }


            return true;
        }

        /// <summary>
        ///  Generate the letters for viewing
        /// </summary>
        /// <param name="letterId">Id of letter</param>
        /// <returns>Results view model</returns>
        public ExportResultsViewModel GenerateLetters(string letterId)
        {
            ExportResultsViewModel retModel = new ExportResultsViewModel();

            int id;
            if (Int32.TryParse(letterId, out id) == false)
            {
                return retModel;
            }

            RiskLetter letter = _riskLetterRepo.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (letter == null)
            {
                return retModel;
            }

            Address homeAddress = letter.Participant.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault();
            retModel.Letters.Add(new Letter()
            {
                LetterText = letter.RiskLetterContent,
                Name = letter.Participant.Title + " " + letter.Participant.FirstName + " " + letter.Participant.LastName,
                AddressLine1 = homeAddress.AddressLine1,
                AddressLine2 = String.IsNullOrEmpty(homeAddress.AddressLine2) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine2,
                AddressLine3 = String.IsNullOrEmpty(homeAddress.AddressLine3) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine3,
                AddressLine4 = String.IsNullOrEmpty(homeAddress.AddressLine4) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine4,
                PostCode = homeAddress.PostCode,
                SentDate = DateTime.Now.ToShortDateString()

            });

            return retModel;
        }

        /// <summary>
        /// Generate the letters for export
        /// </summary>
        /// <param name="model">Export criteria</param>
        /// <returns>Results view model</returns>
        public ExportResultsViewModel GenerateLetters(ExportLettersViewModel model)
        {
            ExportResultsViewModel retModel = new ExportResultsViewModel();

            List<Participant> participants = new List<Participant>();

            // Check if they want all those that are ready for export or just one NHS number
            if (model.AllReady == true)
            {
                participants = _participantRepo.GetAll().Where(x => x.Consented == true && x.FirstName != null && x.SentRisk == false && x.RiskLetters.Count > 0).ToList();
            }
            else
            {
                participants = _participantRepo.GetAll().Where(x => x.NHSNumber == model.NHSNumber && x.Consented == true && x.FirstName != null  && x.RiskLetters.Count > 0).ToList();
            }

            
            foreach (Participant participant in participants)
            {
                Address homeAddress = participant.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault();
                RiskLetter riskLetter = participant.RiskLetters.OrderByDescending(x => x.DateReceived).FirstOrDefault();
                retModel.Letters.Add(new Letter()
                {
                    LetterText = riskLetter.RiskLetterContent,
                    Name = participant.Title + " " + participant.FirstName + " " + participant.LastName,
                    AddressLine1 = homeAddress.AddressLine1,
                    AddressLine2 = String.IsNullOrEmpty(homeAddress.AddressLine2)==true? ExportResources.BLANK_LINE : homeAddress.AddressLine2,
                    AddressLine3 = String.IsNullOrEmpty(homeAddress.AddressLine3) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine3,
                    AddressLine4 = String.IsNullOrEmpty(homeAddress.AddressLine4) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine4,
                    PostCode = homeAddress.PostCode,
                    SentDate = DateTime.Now.ToShortDateString()

                });
            }

            return retModel;
        }

        /// <summary>
        /// Render the passed Razor view and return the HTML as a string.
        /// </summary>
        /// <param name="context">Controller context</param>
        /// <param name="model">View model for the view</param>
        /// <param name="viewName">View name</param>
        /// <returns></returns>
        public string RenderRazorViewToString(ControllerContext context, object model, string viewName)
        {
            context.Controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(context,
                                                                         viewName);
                var viewContext = new ViewContext(context, viewResult.View,
                                             context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(context, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Generate a word document based on the passed HTML.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public MemoryStream GenerateWordDoc(string html)
        {
            MemoryStream generatedDocument = new MemoryStream();

            using (WordprocessingDocument package = WordprocessingDocument.Create(generatedDocument, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = package.MainDocumentPart;
                if (mainPart == null)
                {
                    mainPart = package.AddMainDocumentPart();
                    new Document(new Body()).Save(mainPart);
                }

                // Get the Styles part for this document.
                StyleDefinitionsPart part =
                    package.MainDocumentPart.StyleDefinitionsPart;

                // If the Styles part does not exist, add it and then add the style.
                if (part == null)
                {
                    part = AddStylesPartToPackage(package);
                }


                // Created 2 styles for PROCAS2 letters
                CreateAndAddParagraphStyle(part, "PROCAS2", "PROCAS2_Style", "Normal", "24", "240");
                CreateAndAddParagraphStyle(part, "PROCAS2Head", "PROCAS2_Head_Style", "Normal", "48", "480");

                Body body = mainPart.Document.Body;
                HtmlConverter converter = new HtmlConverter(mainPart);
                converter.HtmlStyles.DefaultStyle = converter.HtmlStyles.GetStyle("PROCAS2_Style");
                converter.ParseHtml(html);

                mainPart.Document.Save();
            }


            return generatedDocument;

        }

        // Create a new paragraph style with the specified style ID, primary style name, and aliases and 
        // add it to the specified style definitions part.
        private void CreateAndAddParagraphStyle(StyleDefinitionsPart styleDefinitionsPart,
            string styleid, string stylename, string basedOn, string fontSize, string lineSize )
        {
            // Access the root element of the styles part.
            Styles styles = styleDefinitionsPart.Styles;
            if (styles == null)
            {
                styleDefinitionsPart.Styles = new Styles();
                styleDefinitionsPart.Styles.Save();
            }

            // Create a new paragraph style element and specify some of the attributes.
            Style style = new Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = styleid,
                CustomStyle = true,
                Default = false,
                StyleName = new StyleName() { Val=stylename },
                StyleParagraphProperties =
                new StyleParagraphProperties(

                        new SpacingBetweenLines() { Before = "0", After = "0", Line = lineSize, LineRule = LineSpacingRuleValues.Exact })
            };



            // Create and add the child elements (properties of the style).
            AutoRedefine autoredefine1 = new AutoRedefine() { Val = OnOffOnlyValues.Off };
            BasedOn basedon1 = new BasedOn() { Val = basedOn };
            LinkedStyle linkedStyle1 = new LinkedStyle() { Val = "OverdueAmountChar" };
            Locked locked1 = new Locked() { Val = OnOffOnlyValues.Off };
            PrimaryStyle primarystyle1 = new PrimaryStyle() { Val = OnOffOnlyValues.On };
            StyleHidden stylehidden1 = new StyleHidden() { Val = OnOffOnlyValues.Off };
            SemiHidden semihidden1 = new SemiHidden() { Val = OnOffOnlyValues.Off };
            StyleName styleName1 = new StyleName() { Val = stylename };
            NextParagraphStyle nextParagraphStyle1 = new NextParagraphStyle() { Val = "Normal" };
            UIPriority uipriority1 = new UIPriority() { Val = 1 };
            UnhideWhenUsed unhidewhenused1 = new UnhideWhenUsed() { Val = OnOffOnlyValues.On };
           
            style.Append(autoredefine1);
            style.Append(basedon1);
            style.Append(linkedStyle1);
            style.Append(locked1);
            style.Append(primarystyle1);
            style.Append(stylehidden1);
            style.Append(semihidden1);
            style.Append(styleName1);
            style.Append(nextParagraphStyle1);
            style.Append(uipriority1);
            style.Append(unhidewhenused1);

            // Create the StyleRunProperties object and specify some of the run properties.
            StyleRunProperties styleRunProperties1 = new StyleRunProperties();

            RunFonts font1 = new RunFonts() { Ascii = "Arial" };

            // Specify a 12 point size.
            FontSize fontSize1 = new FontSize() { Val = fontSize };

            styleRunProperties1.Append(font1);
            styleRunProperties1.Append(fontSize1);


            // Add the run properties to the style.
            style.Append(styleRunProperties1);

            // Add the style to the styles part.
            styles.Append(style);
        }

        // Add a StylesDefinitionsPart to the document.  Returns a reference to it.
        private StyleDefinitionsPart AddStylesPartToPackage(WordprocessingDocument doc)
        {
            StyleDefinitionsPart part;
            part = doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            Styles root = new Styles();
            root.Save(part);
            return part;
        }

    }
}
