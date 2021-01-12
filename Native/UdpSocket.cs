using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Margatroid.Alice.Native
{
    public class UdpSocket : IDisposable
    {
        private bool DisposedValue;
        public FileDescriptor SocketFileDescriptor { get; private set; }

        public UdpSocket()
        {
            SocketFileDescriptor = IO.Check(IO.Socket(IO.AF_INET, IO.SOCK_DGRAM, IO.IPPROTO_IP));
        }

        public void Bind(IPEndPoint endpoint)
        {
            //var option = 1;
            //IO.Check(IO.SetSockOpt(SocketFileDescriptor, IO.SOL_SOCKET, IO.SO_REUSEADDR, ref option, 1));
            var sockAddr = endpoint.ToSockAddr();
            IO.Check(IO.Bind(SocketFileDescriptor, ref sockAddr, Marshal.SizeOf(typeof(SockAddrIn))));
        }

        public byte[] Recv(int size, out IPEndPoint remote)
        {
            var buffer = new byte[size];
            var sockAddr = new SockAddr();
            var addrSize = 16;
            var count = IO.Check(IO.RecvFrom(SocketFileDescriptor, buffer, size, 0, ref sockAddr, ref addrSize));
            var slice = buffer.Take(count).ToArray();
            remote = sockAddr.ToIPEndPoint();
            return slice;
        }

        public void SendTo(IPEndPoint remote, byte[] data)
        {
            var sockAddr = remote.ToSockAddr();
            IO.Check(IO.SendTo(SocketFileDescriptor, data, data.Length, 0, ref sockAddr, Marshal.SizeOf(typeof(SockAddrIn))));
        }

        public Message RecvMessage(out IPEndPoint remote)
        {
            var data = Recv(8192, out remote);
            return MessageHelper.DeserializeMessage(data);
        }

        public void SendMessageTo(IPEndPoint remote, Message message)
        {
            SendTo(remote, MessageHelper.SerializeMessage(message));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                SocketFileDescriptor.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                DisposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~UdpSocket()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
