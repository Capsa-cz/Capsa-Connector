using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capsa_Connector.Controller.Exceptions
{
    class OpenFileCapsaException : Exception
    {
        public OpenFileCapsaException() { }
        public OpenFileCapsaException(string message) : base(message) { }
        public OpenFileCapsaException(string message, Exception innerException) : base(message, innerException) { }
    }
}
