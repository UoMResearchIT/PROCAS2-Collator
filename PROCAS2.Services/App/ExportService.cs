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

            string title = String.IsNullOrEmpty(letter.Participant.Title) == true ? ExportResources.DEFAULT_TITLE : letter.Participant.Title;

            Address homeAddress = letter.Participant.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault();
            retModel.Letters.Add(new Letter()
            {
                LetterText = letter.RiskLetterContent,

                FromAddressLine1 = letter.Participant.ScreeningSite.AddressLine1,
                FromAddressLine2 = String.IsNullOrEmpty(letter.Participant.ScreeningSite.AddressLine2) == true ? ExportResources.BLANK_LINE : letter.Participant.ScreeningSite.AddressLine2,
                FromAddressLine3 = String.IsNullOrEmpty(letter.Participant.ScreeningSite.AddressLine3) == true ? ExportResources.BLANK_LINE : letter.Participant.ScreeningSite.AddressLine3,
                FromAddressLine4 = String.IsNullOrEmpty(letter.Participant.ScreeningSite.AddressLine4) == true ? ExportResources.BLANK_LINE : letter.Participant.ScreeningSite.AddressLine4,
                FromPostCode = letter.Participant.ScreeningSite.PostCode,
                FromName = letter.Participant.ScreeningSite.LetterFrom,
                LogoFile = letter.Participant.ScreeningSite.LogoFileName,
                LogoHeight = letter.Participant.ScreeningSite.LogoHeight,
                LogoFooterLeft = letter.Participant.ScreeningSite.LogoFooterLeft,
                LogoFooterLeftHeight = letter.Participant.ScreeningSite.LogoFooterLeftHeight,
                LogoFooterRight = letter.Participant.ScreeningSite.LogoFooterRight,
                LogoFooterRightHeight = letter.Participant.ScreeningSite.LogoFooterRightHeight,
                SigFile = letter.Participant.ScreeningSite.SigFileName,
                Telephone = letter.Participant.ScreeningSite.Telephone,

                Name = title + " "  + letter.Participant.LastName,
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
                string title = String.IsNullOrEmpty(participant.Title) == true ? ExportResources.DEFAULT_TITLE : participant.Title;

                retModel.Letters.Add(new Letter()
                {
                    LetterText = riskLetter.RiskLetterContent,
                    FromAddressLine1 = participant.ScreeningSite.AddressLine1,
                    FromAddressLine2 = String.IsNullOrEmpty(participant.ScreeningSite.AddressLine2) == true ? ExportResources.BLANK_LINE : participant.ScreeningSite.AddressLine2,
                    FromAddressLine3 = String.IsNullOrEmpty(participant.ScreeningSite.AddressLine3) == true ? ExportResources.BLANK_LINE : participant.ScreeningSite.AddressLine3,
                    FromAddressLine4 = String.IsNullOrEmpty(participant.ScreeningSite.AddressLine4) == true ? ExportResources.BLANK_LINE : participant.ScreeningSite.AddressLine4,
                    FromPostCode = participant.ScreeningSite.PostCode,
                    FromName = participant.ScreeningSite.LetterFrom,
                    LogoFile = participant.ScreeningSite.LogoFileName,
                    LogoHeight = participant.ScreeningSite.LogoHeight,
                    LogoFooterLeft = participant.ScreeningSite.LogoFooterLeft,
                    LogoFooterLeftHeight = participant.ScreeningSite.LogoFooterLeftHeight,
                    LogoFooterRight = participant.ScreeningSite.LogoFooterRight,
                    LogoFooterRightHeight = participant.ScreeningSite.LogoFooterRightHeight,

                    SigFile = participant.ScreeningSite.SigFileName,
                    Telephone = participant.ScreeningSite.Telephone,


                    Name =  title + " " + participant.LastName,
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
                CreateAndAddParagraphStyle(part, "PROCAS2", "PROCAS2_Style", "Normal", "22", "220");
                CreateAndAddParagraphStyle(part, "PROCAS2Head", "PROCAS2_Head_Style", "Normal", "44", "440");

                Body body = mainPart.Document.Body;
                HtmlConverter converter = new HtmlConverter(mainPart);
                converter.HtmlStyles.DefaultStyle = converter.HtmlStyles.GetStyle("PROCAS2_Style");
#if DEBUG
                converter.BaseImageUrl = new Uri(ExportResources.URL_DEV);
#else
                converter.BaseImageUrl = new Uri(ExportResources.URL_PROD);
#endif
                converter.ParseHtml(html);

                SectionProperties sectionProperties1 = mainPart.Document.Body.Descendants<SectionProperties>().FirstOrDefault();
                if (sectionProperties1 == null)
                {
                    sectionProperties1 = new SectionProperties() { };
                    mainPart.Document.Body.Append(sectionProperties1);
                }
                HeaderReference headerReference1 = new HeaderReference() { Type = HeaderFooterValues.Default, Id = "rId2" };
                FooterReference footerReference1 = new FooterReference() { Type = HeaderFooterValues.Default, Id = "rId3" };

                sectionProperties1.InsertAt(headerReference1, 0);
                sectionProperties1.InsertAt(footerReference1, 1);



                mainPart.Document.Save();




               var documentSettingsPart =
            mainPart.AddNewPart
            <DocumentSettingsPart>("rId1");

               GenerateDocumentSettingsPart().Save(documentSettingsPart);

                var firstPageHeaderPart =
                    mainPart.AddNewPart<HeaderPart>("rId2");

                GeneratePageHeaderPart(
                    "First page header").Save(firstPageHeaderPart);

                var firstPageFooterPart =
                    mainPart.AddNewPart<FooterPart>("rId3");

                GeneratePageFooterPart(
                    "First page footer").Save(firstPageFooterPart);

                


            }


            return generatedDocument;

        }

        private Footer GeneratePageFooterPart(string FooterText)
        {
            var element =
                new Footer(
                    new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId() { Val = "Footer" }),
                        new Run(
                            new Text(FooterText))
                    ));

            return element;
        }

        private  Header GeneratePageHeaderPart(string HeaderText)
        {
            var element =
                new Header(
                    new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId() { Val = "Header" }),
                        new Run(
                            new Text(HeaderText))
                    ));

            return element;
        }

        private Settings GenerateDocumentSettingsPart()
        {
            var element =
                new Settings();

            return element;
        }

        void GenerateFooterPartContent(FooterPart part)
        {
            Footer footer1 = new Footer() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "w14 wp14" } };
            footer1.AddNamespaceDeclaration("wpc", "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas");
            footer1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            footer1.AddNamespaceDeclaration("o", "urn:schemas-microsoft-com:office:office");
            footer1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            footer1.AddNamespaceDeclaration("m", "http://schemas.openxmlformats.org/officeDocument/2006/math");
            footer1.AddNamespaceDeclaration("v", "urn:schemas-microsoft-com:vml");
            footer1.AddNamespaceDeclaration("wp14", "http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing");
            footer1.AddNamespaceDeclaration("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing");
            footer1.AddNamespaceDeclaration("w10", "urn:schemas-microsoft-com:office:word");
            footer1.AddNamespaceDeclaration("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            footer1.AddNamespaceDeclaration("w14", "http://schemas.microsoft.com/office/word/2010/wordml");
            footer1.AddNamespaceDeclaration("wpg", "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup");
            footer1.AddNamespaceDeclaration("wpi", "http://schemas.microsoft.com/office/word/2010/wordprocessingInk");
            footer1.AddNamespaceDeclaration("wne", "http://schemas.microsoft.com/office/word/2006/wordml");
            footer1.AddNamespaceDeclaration("wps", "http://schemas.microsoft.com/office/word/2010/wordprocessingShape");

            Paragraph paragraph1 = new Paragraph() { RsidParagraphAddition = "00164C17", RsidRunAdditionDefault = "00164C17" };

            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            ParagraphStyleId paragraphStyleId1 = new ParagraphStyleId() { Val = "Footer" };

            paragraphProperties1.Append(paragraphStyleId1);

            Run run1 = new Run();
            Text text1 = new Text();
            text1.Text = "Footer";

            run1.Append(text1);

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);

            footer1.Append(paragraph1);

            part.Footer = footer1;
        }

        void GenerateHeaderPartContent(HeaderPart part)
        {
            Header header1 = new Header() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "w14 wp14" } };
            header1.AddNamespaceDeclaration("wpc", "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas");
            header1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            header1.AddNamespaceDeclaration("o", "urn:schemas-microsoft-com:office:office");
            header1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            header1.AddNamespaceDeclaration("m", "http://schemas.openxmlformats.org/officeDocument/2006/math");
            header1.AddNamespaceDeclaration("v", "urn:schemas-microsoft-com:vml");
            header1.AddNamespaceDeclaration("wp14", "http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing");
            header1.AddNamespaceDeclaration("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing");
            header1.AddNamespaceDeclaration("w10", "urn:schemas-microsoft-com:office:word");
            header1.AddNamespaceDeclaration("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            header1.AddNamespaceDeclaration("w14", "http://schemas.microsoft.com/office/word/2010/wordml");
            header1.AddNamespaceDeclaration("wpg", "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup");
            header1.AddNamespaceDeclaration("wpi", "http://schemas.microsoft.com/office/word/2010/wordprocessingInk");
            header1.AddNamespaceDeclaration("wne", "http://schemas.microsoft.com/office/word/2006/wordml");
            header1.AddNamespaceDeclaration("wps", "http://schemas.microsoft.com/office/word/2010/wordprocessingShape");

            Paragraph paragraph1 = new Paragraph() { RsidParagraphAddition = "00164C17", RsidRunAdditionDefault = "00164C17" };

            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            ParagraphStyleId paragraphStyleId1 = new ParagraphStyleId() { Val = "Header" };

            paragraphProperties1.Append(paragraphStyleId1);

            Run run1 = new Run();
            Text text1 = new Text();
            text1.Text = "Header";

            run1.Append(text1);

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);

            header1.Append(paragraph1);

            part.Header = header1;
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
            Justification justification = new Justification() { Val = JustificationValues.Both };
           
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
            style.Append(justification);

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
