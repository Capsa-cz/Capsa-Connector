using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capsa_Connector.Controller.Exceptions
{
    class DownloadFileCapsaException : Exception
    {
        public DownloadFileCapsaException() { }
        public DownloadFileCapsaException(string message) : base(message) { }
        public DownloadFileCapsaException(string message, Exception innerException) : base(message, innerException) { }
    }
}
