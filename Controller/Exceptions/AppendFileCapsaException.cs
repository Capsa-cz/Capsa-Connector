using System;

namespace Capsa_Connector.Controller.Exceptions{
    class AppendFileCapsaException : Exception
    {
        public AppendFileCapsaException() { }
        public AppendFileCapsaException(string message) : base(message) { }
        public AppendFileCapsaException(string message, Exception innerException) : base(message, innerException) { }
    }
}
