using System;
using System.Runtime.InteropServices;

namespace Margatroid.Alice.Native
{
    /// <summary>
    /// https://github.com/torvalds/linux/blob/master/include/uapi/linux/if.h
    /// </summary>
	[StructLayout(LayoutKind.Explicit)]
    internal struct InterfaceRequestWithFlags
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ifrn_name;

		[FieldOffset(16)]
		public short ifru_flags;
    }
}