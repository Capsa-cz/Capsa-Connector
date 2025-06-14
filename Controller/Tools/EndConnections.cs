using System;
using System.Threading.Tasks;
using Capsa_Connector.Core;
using Capsa_Connector.Core.FileControl;
using Capsa_Connector.Model;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Controller.Tools;

public static class EndConnections
{
    public static bool EndAllFileConnections()
    {
        try
        {
            foreach (ActiveFile connectedFile in StaticVariables.activeFiles)
            {
                if(connectedFile.hash != null)
                    Task.Run((() => FileControl.CancelFileOnlineEdit(connectedFile.accessToken, connectedFile.hash)));
                else
                    Task.Run((() => FileControl.CancelAndUnlockFile(connectedFile.accessToken)));
            }
            return true;
        }
        catch ( Exception e)
        {
            v("Error while ending all file connections");
            v(e.Message);
            return false;
        }
    }
    public static void EndAllThreads()
    {
        foreach (TaskThreadCommandsStructure taskThreadCommandsStructure in StaticVariables.taskThreadCommands)
        {
            taskThreadCommandsStructure.cancellationTokenSource.Cancel();
        }
    }
}