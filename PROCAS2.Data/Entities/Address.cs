using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class Address
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string AddressLine1 { get; set; }
        [MaxLength(200)]
        public string AddressLine2 { get; set; }
        [MaxLength(200)]
        public string AddressLine3 { get; set; }
        [MaxLength(200)]
        public string AddressLine4 { get; set; }
        [MaxLength(10)]
        public string PostCode { get; set; }
        [MaxLength(200)]
        public string EmailAddress { get; set; }

        public int AddressTypeId { get; set; }



        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; }
    }
}
