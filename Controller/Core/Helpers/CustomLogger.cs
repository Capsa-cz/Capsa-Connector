namespace Capsa_Connector.Controller.Core.Helpers;
using static Capsa_Connector.Core.Tools.Log;

public class CustomLogger: NetSparkleUpdater.Interfaces.ILogger
{
    public void PrintMessage(string message, params object[]? arguments)
    {
        v(message);
    }
}