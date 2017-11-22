using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Models.ViewModels
{

    public class OutputMessage
    {
        public string Message { get; set; }
        public string Pass { get; set; }
        public int Line { get; set; }
        
    }

    public class UploadResultsViewModel
    {
        public UploadResultsViewModel()
        {
            Messages = new List<OutputMessage>();
            DBNoUpdate = false;
        }

        public void AddMessage(int line, string message, string pass)
        {
            OutputMessage mess = new OutputMessage();
            mess.Message = message;
            mess.Pass = pass;
            mess.Line = line;
            Messages.Add(mess);
        }

        public List<OutputMessage> Messages { get; set; }

        public bool DBNoUpdate { get; set; }
    }
}
