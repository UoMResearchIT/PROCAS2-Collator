using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Data
{
    public interface IUnitOfWork
    {
        void Save();
        void Reject();
    }
}
