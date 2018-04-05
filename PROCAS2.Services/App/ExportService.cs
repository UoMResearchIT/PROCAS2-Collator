using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Mvc;
using System.IO;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

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
                LogoHeaderRight = letter.Participant.ScreeningSite.LogoHeaderRight,
                LogoHeaderRightHeight = letter.Participant.ScreeningSite.LogoHeaderRightHeight,
                LogoHeaderRightWidth = letter.Participant.ScreeningSite.LogoHeaderRightWidth,
                LogoFooterLeft = letter.Participant.ScreeningSite.LogoFooterLeft,
                LogoFooterLeftHeight = letter.Participant.ScreeningSite.LogoFooterLeftHeight,
                LogoFooterLeftWidth = letter.Participant.ScreeningSite.LogoFooterLeftWidth,
                LogoFooterRight = letter.Participant.ScreeningSite.LogoFooterRight,
                LogoFooterRightHeight = letter.Participant.ScreeningSite.LogoFooterRightHeight,
                LogoFooterRightWidth = letter.Participant.ScreeningSite.LogoFooterRightWidth,
                Signature = letter.Participant.ScreeningSite.Signature,
                Telephone = letter.Participant.ScreeningSite.Telephone,
                NHSNumber = letter.Participant.NHSNumber,

                Name = title + " " + letter.Participant.LastName,
                AddressLine1 = homeAddress.AddressLine1,
                AddressLine2 = String.IsNullOrEmpty(homeAddress.AddressLine2) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine2,
                AddressLine3 = String.IsNullOrEmpty(homeAddress.AddressLine3) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine3,
                AddressLine4 = String.IsNullOrEmpty(homeAddress.AddressLine4) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine4,
                PostCode = homeAddress.PostCode,
                SentDate = DateTime.Now.ToLongDateString()

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
                participants = _participantRepo.GetAll().Where(x => x.Consented == true && x.FirstName != null && x.SentRisk == false && x.RiskLetters.Count > 0 && x.ScreeningSite.TrustCode == model.SiteToProcess).ToList();
            }
            else
            {
                participants = _participantRepo.GetAll().Where(x => x.NHSNumber == model.NHSNumber && x.Consented == true && x.FirstName != null && x.RiskLetters.Count > 0).ToList();
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
                    LogoHeaderRight = participant.ScreeningSite.LogoHeaderRight,
                    LogoHeaderRightHeight = participant.ScreeningSite.LogoHeaderRightHeight,
                    LogoHeaderRightWidth = participant.ScreeningSite.LogoHeaderRightWidth,
                    LogoFooterLeft = participant.ScreeningSite.LogoFooterLeft,
                    LogoFooterLeftHeight = participant.ScreeningSite.LogoFooterLeftHeight,
                    LogoFooterLeftWidth = participant.ScreeningSite.LogoFooterLeftWidth,
                    LogoFooterRight = participant.ScreeningSite.LogoFooterRight,
                    LogoFooterRightHeight = participant.ScreeningSite.LogoFooterRightHeight,
                    LogoFooterRightWidth = participant.ScreeningSite.LogoFooterRightWidth,

                    Signature = participant.ScreeningSite.Signature,
                    Telephone = participant.ScreeningSite.Telephone,
                    NHSNumber = participant.NHSNumber,

                    Name = title + " " + participant.LastName,
                    AddressLine1 = homeAddress.AddressLine1,
                    AddressLine2 = String.IsNullOrEmpty(homeAddress.AddressLine2) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine2,
                    AddressLine3 = String.IsNullOrEmpty(homeAddress.AddressLine3) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine3,
                    AddressLine4 = String.IsNullOrEmpty(homeAddress.AddressLine4) == true ? ExportResources.BLANK_LINE : homeAddress.AddressLine4,
                    PostCode = homeAddress.PostCode,
                    SentDate = DateTime.Now.ToLongDateString()

                });

                participant.SentRisk = true;
                _participantRepo.Update(participant);
                _unitOfWork.Save();
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
        public MemoryStream GenerateWordDoc(string html, ExportResultsViewModel model)
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

                // convert the html doc into openxml
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

                // header and footer needs to be added
                HeaderReference headerReference1 = new HeaderReference() { Type = HeaderFooterValues.Default, Id = "rId2" };
                FooterReference footerReference1 = new FooterReference() { Type = HeaderFooterValues.Default, Id = "rId3" };

                sectionProperties1.InsertAt(headerReference1, 0);
                sectionProperties1.InsertAt(footerReference1, 1);


                // Need a margin different from the default - this is based on MU NHS Trust template
                PageMargin margin = new PageMargin()
                {
                    Right = (UInt32)new Unit(UnitMetric.Centimeter, 0.83).ValueInDxa,
                    Left = (UInt32)new Unit(UnitMetric.Centimeter, 1.27).ValueInDxa,
                    Top = (Int32)new Unit(UnitMetric.Centimeter, 1.27).ValueInDxa,
                    Bottom = (Int32)new Unit(UnitMetric.Centimeter, 1.27).ValueInDxa,
                    Footer = 0,
                    Header = 720,
                    Gutter = 0
                };
                sectionProperties1.InsertAt(margin, 2);


                mainPart.Document.Save();


                Letter viewLetter = model.Letters[0];

                // Add document settings part
                var documentSettingsPart =
             mainPart.AddNewPart
             <DocumentSettingsPart>("rId1");

                GenerateDocumentSettingsPart().Save(documentSettingsPart);

                // Add header part
                var firstPageHeaderPart =
                    mainPart.AddNewPart<HeaderPart>("rId2");

                Header header = new Header();
                header.Save(firstPageHeaderPart);

                // Add logo image parts to header
                ImagePart imgPartHeaderLeft = AddHeaderImagePart(firstPageHeaderPart, ExportResources.ResourceManager.GetString("UNI_LOGO"));
                ImagePart imgPartHeaderRight = AddHeaderImagePart(firstPageHeaderPart, ExportResources.ResourceManager.GetString(viewLetter.LogoHeaderRight));

                // Generate and save the header
                GeneratePageHeaderPart(firstPageHeaderPart, imgPartHeaderLeft, imgPartHeaderRight,
                    ExportResources.ResourceManager.GetString("PROCAS2_LOGO_HEIGHT_CM"), ExportResources.ResourceManager.GetString("PROCAS2_LOGO_WIDTH_CM"),
                   viewLetter.LogoHeaderRightHeight, viewLetter.LogoHeaderRightWidth).Save(firstPageHeaderPart);


                // Add footer part
                var firstPageFooterPart =
                    mainPart.AddNewPart<FooterPart>("rId3");

                Footer footer = new Footer();
                footer.Save(firstPageFooterPart);

                // Add logo image parts to footer
                ImagePart imgPartFooterLeft = null;

                if (viewLetter.LogoFooterLeft != null)
                {
                    imgPartFooterLeft = AddFooterImagePart(firstPageFooterPart, ExportResources.ResourceManager.GetString(viewLetter.LogoFooterLeft));
                }

                ImagePart imgPartFooterRight = null;

                if (viewLetter.LogoFooterRight != null)
                {
                    imgPartFooterRight = AddFooterImagePart(firstPageFooterPart, ExportResources.ResourceManager.GetString(viewLetter.LogoFooterRight));
                }

                // Generate and save the footer
                GeneratePageFooterPart(
                    firstPageFooterPart, imgPartFooterLeft, imgPartFooterRight,
                    viewLetter.LogoFooterLeftHeight, viewLetter.LogoFooterLeftWidth,
                    viewLetter.LogoFooterRightHeight, viewLetter.LogoFooterRightWidth).Save(firstPageFooterPart);



                mainPart.Document.Save();

            }


            return generatedDocument;

        }

        /// <summary>
        /// Add image part to the header
        /// </summary>
        /// <param name="headerPart">Header part</param>
        /// <param name="imageString">Base 64 string containing the image</param>
        /// <returns></returns>
        private ImagePart AddHeaderImagePart(HeaderPart headerPart, string imageString)
        {

            ImagePart imagePart = headerPart.AddImagePart(ImagePartType.Png);


            if (String.IsNullOrEmpty(imageString) == false)
            {
                var bytes = Convert.FromBase64String(imageString);
                var contents = new MemoryStream(bytes);
                imagePart.FeedData(contents);
                contents.Close();
            }
            return imagePart;
        }

        /// <summary>
        /// Add image part to the footer
        /// </summary>
        /// <param name="footerPart">footer part</param>
        /// <param name="imageString">Base64 string to add to image</param>
        /// <returns></returns>
        private ImagePart AddFooterImagePart(FooterPart footerPart, string imageString)
        {

            ImagePart imagePart = footerPart.AddImagePart(ImagePartType.Png);

            if (String.IsNullOrEmpty(imageString) == false)
            {
                var bytes = Convert.FromBase64String(imageString);
                var contents = new MemoryStream(bytes);
                imagePart.FeedData(contents);
                contents.Close();
            }

            return imagePart;
        }

        /// <summary>
        /// Generate the footer 
        /// </summary>
        /// <param name="footerPart">Footer part</param>
        /// <param name="imgPartFooterLeft">Image part of the left logo</param>
        /// <param name="imgPartFooterRight">Image part of the right logo</param>
        /// <param name="footerLeftHeight">height of left logo</param>
        /// <param name="footerLeftWidth">width of left logo</param>
        /// <param name="footerRightHeight">height of right logo</param>
        /// <param name="footerRightWidth">width of right logo</param>
        /// <returns></returns>
        private Footer GeneratePageFooterPart(FooterPart footerPart, ImagePart imgPartFooterLeft, ImagePart imgPartFooterRight,
                string footerLeftHeight, string footerLeftWidth, string footerRightHeight, string footerRightWidth)
        {
            Footer element = footerPart.Footer;


            // Style it by using tables
            Table table = new Table(
                new TableProperties(new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct })
                );

            // table row
            var tr = new TableRow();

            // left cell
            var tcLeft = new TableCell();
            tcLeft.Append(
                new TableCellProperties(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Bottom }));

            if (imgPartFooterLeft != null)
            {
                tcLeft.Append(new Paragraph(new Run(CreateDrawingInFooter(footerPart, imgPartFooterLeft, footerLeftHeight, footerLeftWidth))));
            }
            else
            {
                tcLeft.Append(new Paragraph());
            }
            // Assume you want columns that are automatically sized.
            tcLeft.Append(new TableCellProperties(
                new TableCellWidth { Type = TableWidthUnitValues.Auto }));

            tr.Append(tcLeft);

            // right cell
            var tcRight = new TableCell();
            tcRight.Append(
                new TableCellProperties(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Bottom }));

            if (imgPartFooterRight != null)
            {
                tcRight.Append(
                new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                new Run(CreateDrawingInFooter(footerPart, imgPartFooterRight, footerRightHeight, footerRightWidth))));
            }
            else
            {
                tcRight.Append(
                new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Right })
                ));
            }

            // Assume you want columns that are automatically sized.
            tcLeft.Append(new TableCellProperties(
                new TableCellWidth { Type = TableWidthUnitValues.Auto }));

            tr.Append(tcRight);

            table.Append(tr);

            element.Append(table);

            return element;
        }

        /// <summary>
        /// Generate the header
        /// </summary>
        /// <param name="headerPart">header part</param>
        /// <param name="imgPartHeaderLeft">image part for the left logo</param>
        /// <param name="imgPartHeaderRight">image part for the right logo</param>
        /// <param name="headerLeftHeight">height of left logo</param>
        /// <param name="headerLeftWidth">width of left logo</param>
        /// <param name="headerRightHeight">height of right logo</param>
        /// <param name="headerRightWidth">width of right logo</param>
        /// <returns></returns>
        private Header GeneratePageHeaderPart(HeaderPart headerPart, ImagePart imgPartHeaderLeft, ImagePart imgPartHeaderRight,
            string headerLeftHeight, string headerLeftWidth, string headerRightHeight, string headerRightWidth)
        {

            Header element = headerPart.Header;

            // Style by using tables
            Table table = new Table(
                new TableProperties(new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct })
                );

            // table row
            var tr = new TableRow();

            // left cell
            var tcLeft = new TableCell();
            tcLeft.Append(new Paragraph(
                new Run(
                    CreateDrawingInHeader(headerPart, imgPartHeaderLeft, headerLeftHeight, headerLeftWidth)
                    )
                ));

            // Assume you want columns that are automatically sized.
            tcLeft.Append(new TableCellProperties(
                new TableCellWidth { Type = TableWidthUnitValues.Auto }));

            tr.Append(tcLeft);


            // right cell
            var tcRight = new TableCell();
            tcRight.Append(new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                new Run(
                            CreateDrawingInHeader(headerPart, imgPartHeaderRight, headerRightHeight, headerRightWidth)
                            )
                            ));

            // Assume you want columns that are automatically sized.
            tcLeft.Append(new TableCellProperties(
                new TableCellWidth { Type = TableWidthUnitValues.Auto }));

            tr.Append(tcRight);

            table.Append(tr);

            element.Append(table);

            return element;
        }

        /// <summary>
        /// Create the logo drawing in header
        /// </summary>
        /// <param name="headerPart">header part</param>
        /// <param name="imgPart">image part</param>
        /// <param name="headerImageHeight">image height</param>
        /// <param name="headerImageWidth">image width</param>
        /// <returns></returns>
        private Drawing CreateDrawingInHeader(HeaderPart headerPart, ImagePart imgPart,
            string headerImageHeight, string headerImageWidth)
        {
            UInt32Value drawingObjId;
            UInt32Value imageObjId;

            drawingObjId = 1; // 1 is the minimum ID set by MS Office.
            imageObjId = 1;
            foreach (var d in headerPart.Header.Descendants<Drawing>())
            {
                if (d.Inline == null) continue;
                if (d.Inline.DocProperties.Id > drawingObjId) drawingObjId = d.Inline.DocProperties.Id;

                var pic = d.Inline.Graphic.GraphicData.GetFirstChild<PIC.Picture>();
                if (pic != null)
                {
                    var nvPr = pic.GetFirstChild<PIC.NonVisualPictureProperties>();
                    if (nvPr != null && nvPr.NonVisualDrawingProperties.Id > imageObjId)
                        imageObjId = nvPr.NonVisualDrawingProperties.Id;
                }
            }
            if (drawingObjId > 1) drawingObjId++;
            if (imageObjId > 1) imageObjId++;

            long heightInEmus = new Unit(UnitMetric.Centimeter, Convert.ToDouble(headerImageHeight)).ValueInEmus;
            long widthInEmus = new Unit(UnitMetric.Centimeter, Convert.ToDouble(headerImageWidth)).ValueInEmus;

            return new Drawing(
             new DW.Inline(
                 new DW.Extent() { Cx = widthInEmus, Cy = heightInEmus },
                 new DW.EffectExtent()
                 {
                     LeftEdge = 0L,
                     TopEdge = 0L,
                     RightEdge = 0L,
                     BottomEdge = 0L
                 },
                 new DW.DocProperties()
                 {
                     Id = (UInt32Value)drawingObjId,
                     Name = imgPart.Uri.ToString()

                 },
                 new DW.NonVisualGraphicFrameDrawingProperties(
                     new A.GraphicFrameLocks() { NoChangeAspect = true }),
                 new A.Graphic(
                     new A.GraphicData(
                         new PIC.Picture(
                             new PIC.NonVisualPictureProperties(
                                 new PIC.NonVisualDrawingProperties()
                                 {
                                     Id = (UInt32Value)imageObjId,
                                     Name = imgPart.Uri.ToString()
                                 },
                             new PIC.NonVisualPictureDrawingProperties(
                                    new A.PictureLocks() { NoChangeAspect = true, NoChangeArrowheads = true })
                             ),

                             new PIC.BlipFill(
                                 new A.Blip()
                                 {
                                     Embed = headerPart.GetIdOfPart(imgPart)

                                 },
                                 new A.Stretch(
                                     new A.FillRectangle())),
                             new PIC.ShapeProperties(
                                 new A.Transform2D(
                                     new A.Offset() { X = 0L, Y = 0L },
                                     new A.Extents() { Cx = widthInEmus, Cy = heightInEmus }),
                                 new A.PresetGeometry(
                                     new A.AdjustValueList()
                                 )
                                 { Preset = A.ShapeTypeValues.Rectangle }
                                 )
                             { BlackWhiteMode = A.BlackWhiteModeValues.Auto }
                             )


                     )
                     { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
             )
             {
                 DistanceFromTop = (UInt32Value)0U,
                 DistanceFromBottom = (UInt32Value)0U,
                 DistanceFromLeft = (UInt32Value)0U,
                 DistanceFromRight = (UInt32Value)0U

             });
        }

        /// <summary>
        /// Create the logo drawing in footer
        /// </summary>
        /// <param name="headerPart">footer part</param>
        /// <param name="imgPart">image part</param>
        /// <param name="headerImageHeight">image height</param>
        /// <param name="headerImageWidth">image width</param>
        /// <returns></returns>
        private Drawing CreateDrawingInFooter(FooterPart footerPart, ImagePart imgPart,
            string footerImageHeight, string footerImageWidth)
        {
            UInt32Value drawingObjId;
            UInt32Value imageObjId;

            drawingObjId = 1; // 1 is the minimum ID set by MS Office.
            imageObjId = 1;
            foreach (var d in footerPart.Footer.Descendants<Drawing>())
            {
                if (d.Inline == null) continue;
                if (d.Inline.DocProperties.Id > drawingObjId) drawingObjId = d.Inline.DocProperties.Id;

                var pic = d.Inline.Graphic.GraphicData.GetFirstChild<PIC.Picture>();
                if (pic != null)
                {
                    var nvPr = pic.GetFirstChild<PIC.NonVisualPictureProperties>();
                    if (nvPr != null && nvPr.NonVisualDrawingProperties.Id > imageObjId)
                        imageObjId = nvPr.NonVisualDrawingProperties.Id;
                }
            }
            if (drawingObjId > 1) drawingObjId++;
            if (imageObjId > 1) imageObjId++;

            long heightInEmus = new Unit(UnitMetric.Centimeter, Convert.ToDouble(footerImageHeight)).ValueInEmus;
            long widthInEmus = new Unit(UnitMetric.Centimeter, Convert.ToDouble(footerImageWidth)).ValueInEmus;

            return new Drawing(
             new DW.Inline(
                 new DW.Extent() { Cx = widthInEmus, Cy = heightInEmus },
                 new DW.EffectExtent()
                 {
                     LeftEdge = 0L,
                     TopEdge = 0L,
                     RightEdge = 0L,
                     BottomEdge = 0L
                 },
                 new DW.DocProperties()
                 {
                     Id = (UInt32Value)drawingObjId,
                     Name = imgPart.Uri.ToString()

                 },
                 new DW.NonVisualGraphicFrameDrawingProperties(
                     new A.GraphicFrameLocks() { NoChangeAspect = true }),
                 new A.Graphic(
                     new A.GraphicData(
                         new PIC.Picture(
                             new PIC.NonVisualPictureProperties(
                                 new PIC.NonVisualDrawingProperties()
                                 {
                                     Id = (UInt32Value)imageObjId,
                                     Name = imgPart.Uri.ToString()
                                 },
                             new PIC.NonVisualPictureDrawingProperties(
                                    new A.PictureLocks() { NoChangeAspect = true, NoChangeArrowheads = true })
                             ),

                             new PIC.BlipFill(
                                 new A.Blip()
                                 {
                                     Embed = footerPart.GetIdOfPart(imgPart)

                                 },
                                 new A.Stretch(
                                     new A.FillRectangle())),
                             new PIC.ShapeProperties(
                                 new A.Transform2D(
                                     new A.Offset() { X = 0L, Y = 0L },
                                     new A.Extents() { Cx = widthInEmus, Cy = heightInEmus }),
                                 new A.PresetGeometry(
                                     new A.AdjustValueList()
                                 )
                                 { Preset = A.ShapeTypeValues.Rectangle }
                                 )
                             { BlackWhiteMode = A.BlackWhiteModeValues.Auto }
                             )


                     )
                     { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
             )
             {
                 DistanceFromTop = (UInt32Value)0U,
                 DistanceFromBottom = (UInt32Value)0U,
                 DistanceFromLeft = (UInt32Value)0U,
                 DistanceFromRight = (UInt32Value)0U

             });
        }

        private Settings GenerateDocumentSettingsPart()
        {
            var element =
                new Settings();

            return element;
        }



        // Create a new paragraph style with the specified style ID, primary style name, and aliases and 
        // add it to the specified style definitions part.
        private void CreateAndAddParagraphStyle(StyleDefinitionsPart styleDefinitionsPart,
            string styleid, string stylename, string basedOn, string fontSize, string lineSize)
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
                StyleName = new StyleName() { Val = stylename },
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
