namespace Capsa_Connector.Controller.Tools;

using System;
using System.Runtime.InteropServices;
using System.Text;
using static Capsa_Connector.Core.Tools.Log;
using System;
using System.Runtime.InteropServices;

public static class CredentialManager
{
    public static bool SaveCredential(string targetName, string userName, string password)
    {
        const int CRED_PERSIST_LOCAL_MACHINE = 2;

        var credential = new CREDENTIAL
        {
            Flags = 0,
            Type = CRED_TYPE.GENERIC,
            TargetName = targetName,
            Comment = "",
            CredentialBlobSize = (password.Length + 1) * 2, // Unicode = 2 bajty na znak
            CredentialBlob = Marshal.StringToCoTaskMemUni(password),
            Persist = CRED_PERSIST_LOCAL_MACHINE,
            AttributeCount = 0,
            Attributes = IntPtr.Zero,
            TargetAlias = null,
            UserName = userName
        };

        bool result = CredWrite(ref credential, 0);
        Marshal.FreeCoTaskMem(credential.CredentialBlob);

        if (!result)
        {
            int err = Marshal.GetLastWin32Error();
            Console.WriteLine($"[CredentialManager] Kurva, zápis do credentialů selhal. Chyba: {err}");
        }

        return result;
    }

    private enum CRED_TYPE : int
    {
        GENERIC = 1
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public int Flags;
        public CRED_TYPE Type;
        public string TargetName;
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CredWrite([In] ref CREDENTIAL Credential, [In] uint Flags);
}
