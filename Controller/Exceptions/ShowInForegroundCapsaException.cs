using System;

namespace Capsa_Connector.Controller.Exceptions;

class ShowInForegroundCapsaException: Exception
{
    public ShowInForegroundCapsaException() { }
    public ShowInForegroundCapsaException(string message) : base(message) { }
    public ShowInForegroundCapsaException(string message, Exception innerException) : base(message, innerException) { }
}