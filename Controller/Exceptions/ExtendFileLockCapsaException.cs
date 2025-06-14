using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Capsa_Connector.Controller.Exceptions
{
    class ExtendFileLockCapsaException : Exception
    {
        public HttpStatusCode? httpStatusCode { get; set; }
        public ExtendFileLockCapsaException() { }
        public ExtendFileLockCapsaException(string message) : base(message) { }
        public ExtendFileLockCapsaException(string message, HttpStatusCode code) : base(message)
        {
            httpStatusCode = code;
        }

        public ExtendFileLockCapsaException(string message, Exception innerException) : base(message, innerException) { }
        public ExtendFileLockCapsaException(string message, HttpStatusCode code, Exception innerException) : base(message, innerException)
        {
            httpStatusCode = code;
        }

    }
}
