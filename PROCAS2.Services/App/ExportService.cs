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

namespace PROCAS2.Services.App
{
    public class ExportService:IExportService
    {
        private IUnitOfWork _unitOfWork;
        private IGenericRepository<Participant> _participantRepo;

        public ExportService(IGenericRepository<Participant> participantRepo,
                            IUnitOfWork unitOfWork)
        {
            _participantRepo = participantRepo;
            _unitOfWork = unitOfWork;
        }


        public ExportResultsViewModel GenerateLetters(ExportLettersViewModel model)
        {
            ExportResultsViewModel retModel = new ExportResultsViewModel();

            List<Participant> participants = _participantRepo.GetAll().Where(x => x.Consented == true && x.FirstName != null && x.SentRisk == false).ToList();
            foreach(Participant participant in participants)
            {
                Address homeAddress = participant.Addresses.Where(x => x.AddressType.Name == "HOME").FirstOrDefault();
                retModel.Letters.Add(new Letter()
                {
                    LetterText = participant.RiskLetterContent,
                    Name = participant.Title + " " + participant.FirstName + " " + participant.LastName,
                    AddressLine1 = homeAddress.AddressLine1,
                    AddressLine2 = homeAddress.AddressLine2,
                    AddressLine3 = homeAddress.AddressLine3,
                    AddressLine4 = homeAddress.AddressLine4,
                    PostCode = homeAddress.PostCode,
                    SentDate = DateTime.Now.ToShortDateString()

                });
            }

            return retModel;
        }


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

                // Set up a variable to hold the style ID.
                string parastyleid = "PROCAS2";

                // Create and add a paragraph style to the specified styles part 
                // with the specified style ID, style name and aliases.
                CreateAndAddParagraphStyle(part,
                    parastyleid, "PROCAS2 Style");
                //var part = package.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
                //var root = new Styles();
                //root.Save(part);

                //var style = part.Styles.OfType<Style>().Where(i => i.StyleId == "Normal").FirstOrDefault();
                ////var styleid =  stylepart.GetPartById("Heading1");
                //var color1 = new Color() { Val = "FF0000" };
                //style.StyleRunProperties.Append(color1);

                Body body = mainPart.Document.Body;
                HtmlConverter converter = new HtmlConverter(mainPart);
                converter.HtmlStyles.DefaultStyle = converter.HtmlStyles.GetStyle("PROCAS2");
                converter.ParseHtml(html);
                
                
                converter.HtmlStyles.StyleMissing += delegate (object sender, StyleEventArgs e)
                   {
                       Console.WriteLine(e.Name);
                    };

                

                    mainPart.Document.Save();
                }

            
            return generatedDocument;

        }

        // Create a new paragraph style with the specified style ID, primary style name, and aliases and 
        // add it to the specified style definitions part.
        public static void CreateAndAddParagraphStyle(StyleDefinitionsPart styleDefinitionsPart,
            string styleid, string stylename, string aliases = "")
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
                StyleParagraphProperties = 
                new StyleParagraphProperties(
     
                        new SpacingBetweenLines() { Before = "0", After = "0", Line = "240", LineRule = LineSpacingRuleValues.Exact })
            };
            


         // Create and add the child elements (properties of the style).
         Aliases aliases1 = new Aliases() { Val = aliases };
            AutoRedefine autoredefine1 = new AutoRedefine() { Val = OnOffOnlyValues.Off };
            BasedOn basedon1 = new BasedOn() { Val = "Normal" };
            LinkedStyle linkedStyle1 = new LinkedStyle() { Val = "OverdueAmountChar" };
            Locked locked1 = new Locked() { Val = OnOffOnlyValues.Off };
            PrimaryStyle primarystyle1 = new PrimaryStyle() { Val = OnOffOnlyValues.On };
            StyleHidden stylehidden1 = new StyleHidden() { Val = OnOffOnlyValues.Off };
            SemiHidden semihidden1 = new SemiHidden() { Val = OnOffOnlyValues.Off };
            StyleName styleName1 = new StyleName() { Val = stylename };
            NextParagraphStyle nextParagraphStyle1 = new NextParagraphStyle() { Val = "Normal" };
            UIPriority uipriority1 = new UIPriority() { Val = 1 };
            UnhideWhenUsed unhidewhenused1 = new UnhideWhenUsed() { Val = OnOffOnlyValues.On };
            if (aliases != "")
                style.Append(aliases1);
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
            FontSize fontSize1 = new FontSize() { Val = "24" };
            
            styleRunProperties1.Append(font1);
            styleRunProperties1.Append(fontSize1);
            

            // Add the run properties to the style.
            style.Append(styleRunProperties1);

            // Add the style to the styles part.
            styles.Append(style);
        }

        // Add a StylesDefinitionsPart to the document.  Returns a reference to it.
        public static StyleDefinitionsPart AddStylesPartToPackage(WordprocessingDocument doc)
        {
            StyleDefinitionsPart part;
            part = doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            Styles root = new Styles();
            root.Save(part);
            return part;
        }

    }
}
