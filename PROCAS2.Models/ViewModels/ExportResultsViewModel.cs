using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Models.ViewModels
{
    public class ExportResultsViewModel
    {
        public ExportResultsViewModel()
        {
            Letters = new List<Letter>();
        }

        public List<Letter> Letters { get; set; }
    }


    public class Letter
    {
        public Letter()
        {

        }

        public string Name { get; set; }
        public string LetterText { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string PostCode { get; set; }
        public string SentDate { get; set; }

        public string FromAddressLine1 { get; set; }
        public string FromAddressLine2 { get; set; }
        public string FromAddressLine3 { get; set; }
        public string FromAddressLine4 { get; set; }
        public string FromPostCode { get; set; }

        public string FromName { get; set; }
        public string LogoFile { get; set; }
        public string SigFile { get; set; }
    }

}
