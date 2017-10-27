using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Data.Entities
{
    public class AddressType
    {

        public int Id { get; set; }

        [MaxLength(20)]
        public string Name { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
    }
}
