using System.Runtime.InteropServices;

namespace Margatroid.Alice.Native
{
    /// <summary>
    /// https://github.com/torvalds/linux/blob/master/include/uapi/linux/in.h
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SockAddrIn
    {
        public ushort sin_family;

        public ushort sin_port;

        public uint sin_addr;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string sin_zero;
    }
}