using System;
using System.Runtime.InteropServices;

namespace Margatroid.Alice.Native
{
    /// <summary>
    /// https://github.com/torvalds/linux/blob/master/include/uapi/linux/if.h
    /// </summary>
	[StructLayout(LayoutKind.Sequential)]
    internal struct InterfaceRequestWithAddress
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ifrn_name;

        public SockAddr if_addr;
    }
}