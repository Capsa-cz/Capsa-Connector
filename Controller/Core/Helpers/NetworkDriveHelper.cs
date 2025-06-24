namespace Capsa_Connector.Controller.Core.Helpers;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class NetworkDriveHelper
{
    [DllImport("mpr.dll", CharSet = CharSet.Auto)]
    private static extern int WNetOpenEnum(
        int dwScope,
        int dwType,
        int dwUsage,
        IntPtr lpNetResource,
        out IntPtr lphEnum);

    [DllImport("mpr.dll", CharSet = CharSet.Auto)]
    private static extern int WNetEnumResource(
        IntPtr hEnum,
        ref int lpcCount,
        IntPtr lpBuffer,
        ref int lpBufferSize);

    [DllImport("mpr.dll")]
    private static extern int WNetCloseEnum(IntPtr hEnum);

    [StructLayout(LayoutKind.Sequential)]
    private class NETRESOURCE
    {
        public int dwScope;
        public int dwType;
        public int dwDisplayType;
        public int dwUsage;
        public string lpLocalName;
        public string lpRemoteName;
        public string lpComment;
        public string lpProvider;
    }

    public static List<string> GetMappedDriveLetters()
    {
        List<string> drives = new();
        IntPtr handle;
        int result = WNetOpenEnum(0x00000003, 1, 0, IntPtr.Zero, out handle); // RESOURCE_REMEMBERED, RESOURCETYPE_DISK

        if (result != 0)
            return drives;

        int entries = -1;
        int bufferSize = 16384;
        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            result = WNetEnumResource(handle, ref entries, buffer, ref bufferSize);
            if (result == 0)
            {
                IntPtr current = buffer;
                int structSize = Marshal.SizeOf(typeof(NETRESOURCE));

                for (int i = 0; i < entries; i++)
                {
                    NETRESOURCE nr = (NETRESOURCE)Marshal.PtrToStructure(current, typeof(NETRESOURCE));
                    current = (IntPtr)(current.ToInt64() + structSize);

                    if (!string.IsNullOrEmpty(nr.lpLocalName))
                    {
                        drives.Add(nr.lpLocalName);
                    }
                }
            }
        }
        finally
        {
            WNetCloseEnum(handle);
            Marshal.FreeHGlobal(buffer);
        }

        return drives;
    }
}
