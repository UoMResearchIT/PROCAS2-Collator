using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

namespace PROCAS2.Data.AzureTable
{
    public class Invited :TableEntity
    {

        public Invited() { }

        public Invited(string studyNumber, string invitedString)
        {
            this.PartitionKey = invitedString;
            this.RowKey = studyNumber;
        }

        public string HashedPatientId { get; set; }
       
    }
}
