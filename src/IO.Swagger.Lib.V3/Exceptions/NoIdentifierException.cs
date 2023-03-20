﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Swagger.Lib.V3.Exceptions
{
    public class NoIdentifierException : Exception
    {
        public NoIdentifierException(string fieldName) : base($"No indentier found in {fieldName}.")
        {

        }
    }
}
