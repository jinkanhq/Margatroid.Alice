using CsvHelper;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Margatroid.Alice.Native
{
    internal static class InterfaceHelper
    {
        /// <summary>
        /// https://github.com/torvalds/linux/blob/master/include/uapi/linux/if.h
        /// </summary>
        internal const short IFF_UP = 1;

        public static string GetDefaultInterface()
        {
            using var reader = new StreamReader("/proc/net/route");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = "\t";
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                var interfaceName = csv.GetField("Iface");
                var flags = csv.GetField<int>("Flags");
                if (flags == 3) return interfaceName;
            }
            throw new AliceException("Cannot acquire default interface");
        }

        public static IPAddress GetInterfaceAddress(string interfaceName)
        {
            var ifReq = new InterfaceRequestWithAddress();
            ifReq.ifrn_name = interfaceName;
            using var socketFileDescriptor = IO.Socket(IO.AF_INET, IO.SOCK_DGRAM, IO.IPPROTO_IP);
            IO.IOCtl(socketFileDescriptor, IO.SIOCGIFADDR, ref ifReq);
            var addressBytes = Enumerable.Range(2, 4).Select(i => ifReq.if_addr.sa_data[i]).ToArray();
            return new IPAddress(addressBytes);
        }

        public static void SetInterfaceAddress(string interfaceName, IPAddress address, bool isMask)
        {
            SetInterfaceAddress(interfaceName, address.ToString(), isMask);
        }

        public static void SetInterfaceAddress(string interfaceName, string address, bool isMask)
        {
            var ifReq = new InterfaceRequestWithAddress();
            var sockAddrIn = new SockAddrIn();
            sockAddrIn.sin_family = IO.AF_INET;
            IO.Inet_pton(IO.AF_INET, address.ToString(), ref sockAddrIn.sin_addr);
            IntPtr sockAddrInPtr = Marshal.AllocHGlobal(Marshal.SizeOf(sockAddrIn));
            Marshal.StructureToPtr(sockAddrIn, sockAddrInPtr, true);
            var sockAddr = Marshal.PtrToStructure<SockAddr>(sockAddrInPtr);
            Marshal.FreeHGlobal(sockAddrInPtr);
            ifReq.ifrn_name = interfaceName;
            ifReq.if_addr = sockAddr;
            using var socketFileDescriptor = IO.Socket(IO.AF_INET, IO.SOCK_DGRAM, IO.IPPROTO_IP);
            var request = isMask ? IO.SIOCSIFNETMASK : IO.SIOCSIFADDR;
            // Remove IPv6 address
            IO.IOCtl(socketFileDescriptor, request, ref ifReq);
            var p = new Process();
            p.StartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "ip",
                Arguments = $"-6 addr flush {interfaceName}"
            };
            p.Start();
        }

        public static void SetInterfaceUp(string interfaceName)
        {
            var ifReq = new InterfaceRequestWithFlags();
            ifReq.ifrn_name = interfaceName;
            ifReq.ifru_flags |= IFF_UP;
            using var socketFileDescriptor = IO.Socket(IO.AF_INET, IO.SOCK_DGRAM, IO.IPPROTO_IP);
            IO.Check(IO.IOCtl(socketFileDescriptor, IO.SIOCSIFFLAGS, ref ifReq));
        }

        public static IPAddress GetLocalAddress()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 49001);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint.Address;
        }

        public static bool IsAddressInNetwork(IPAddress address, string cidr)
        {
            var parts = cidr.Split('/');
            return IsAddressInNetwork(address, IPAddress.Parse(parts[0]), int.Parse(parts[1]));
        }

        public static bool IsAddressInNetwork(IPAddress address, IPAddress network, int mask)
        {
            int addressInt = BitConverter.ToInt32(address.GetAddressBytes(), 0);
            int networkInt = BitConverter.ToInt32(network.GetAddressBytes(), 0);
            int maskInt = IPAddress.HostToNetworkOrder(-1 << (32 - mask));
            return ((addressInt & maskInt) == (networkInt & maskInt));
        }
    }
}
