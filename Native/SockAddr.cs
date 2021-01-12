using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Margatroid.Alice.Native
{
    /// <summary>
    /// https://github.com/torvalds/linux/blob/master/include/linux/socket.h
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SockAddr
    {
        public ushort sa_family;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public byte[] sa_data;

        public IPEndPoint ToIPEndPoint()
        {
            IntPtr sockAddrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SockAddr)));
            Marshal.StructureToPtr(this, sockAddrPtr, true);
            var sockAddrIn = Marshal.PtrToStructure<SockAddrIn>(sockAddrPtr);
            Marshal.FreeHGlobal(sockAddrPtr);
            var port = (ushort)IPAddress.NetworkToHostOrder((short)sockAddrIn.sin_port);
            return new IPEndPoint(new IPAddress(sockAddrIn.sin_addr), port);
        }
    }
}