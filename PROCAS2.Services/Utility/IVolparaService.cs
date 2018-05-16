using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Services.Utility
{
    public interface IVolparaService
    {
        List<string> ProcessScreeningMessage(string message);
    }
}
