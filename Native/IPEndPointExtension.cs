using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Margatroid.Alice.Native
{
    internal static class IPEndPointExtension
    {
        internal static SockAddr ToSockAddr(this IPEndPoint endPoint)
        {
            var sockAddrIn = new SockAddrIn();
            sockAddrIn.sin_family = IO.AF_INET;
            sockAddrIn.sin_port = (ushort)IPAddress.HostToNetworkOrder((short)endPoint.Port);
            IO.Inet_pton(IO.AF_INET, endPoint.Address.ToString(), ref sockAddrIn.sin_addr);
            IntPtr sockAddrInPtr = Marshal.AllocHGlobal(Marshal.SizeOf(sockAddrIn));
            Marshal.StructureToPtr(sockAddrIn, sockAddrInPtr, true);
            var sockAddr = Marshal.PtrToStructure<SockAddr>(sockAddrInPtr);
            Marshal.FreeHGlobal(sockAddrInPtr);
            return sockAddr;
        }
    }
}
