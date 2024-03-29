﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROCAS2.Services.Utility
{
    public interface IHashingService
    {
        string CreateHash(string password);
        bool ValidatePassword(string password, string goodHash);

        string CreateNHSHash(string NHSNumber);
        bool ValidateNHSNumber(string NHSNumber, string goodHash);

        string CreateScreenHash(string screeningNumber);
    }
}
