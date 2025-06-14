using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capsa_Connector.Controller.Exceptions
{
    class ReadingLocalEditHistoryCapsaException : Exception
    {
        public ReadingLocalEditHistoryCapsaException() { }
        public ReadingLocalEditHistoryCapsaException(string message) : base(message) { }
        public ReadingLocalEditHistoryCapsaException(string message, Exception innerException) : base(message, innerException) { }
    }
}
