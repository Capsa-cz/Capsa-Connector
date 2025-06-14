using System.Threading;
using System.Threading.Tasks;
using Capsa_Connector.Core.Bases;
using Capsa_Connector.ViewModel;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Controller.Tools;

public class VPNConnector
{
    public static int TestVPN()
    {
        return ConnectToVPN("VPN", "username", "password", "domain");
        //Thread.Sleep(5000);
        //DisconnectFromVPN("VPN");
    }
    public static int ConnectToVPN(string vpnName, string username, string password, string domain)
    {
        string arg = $"{vpnName} {username} {password} /domain:{domain}";
        v($"VPN Command: rasdial.exe {arg}");
        
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo = new System.Diagnostics.ProcessStartInfo("rasdial.exe", arg);
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.ErrorDialog = false;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        process.Start();
        process.WaitForExit();
        return process.ExitCode;
    }
    public static void DisconnectFromVPN(string vpnName)
    {
        System.Diagnostics.Process.Start("rasdial.exe", $"{vpnName} /disconnect");
    }
    
}