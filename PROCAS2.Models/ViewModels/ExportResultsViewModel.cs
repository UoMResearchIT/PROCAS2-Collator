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
    }

}
