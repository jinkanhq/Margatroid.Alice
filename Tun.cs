using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Margatroid.Alice.Native;

namespace Margatroid.Alice
{
    class Tun : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// https://github.com/torvalds/linux/blob/master/include/uapi/linux/if_tun.h
        /// </summary>
        internal const short IFF_TUN = 1;
        /// <summary>
        /// https://github.com/torvalds/linux/blob/master/include/uapi/linux/if_tun.h
        /// </summary>
        internal const short IFF_TAP = 2;

        /// <summary>
        /// https://github.com/torvalds/linux/blob/master/include/uapi/linux/if_tun.h
        /// </summary>
        internal static uint TUNSETIFF = IO._IOW('T', 202, typeof(int));
        /// <summary>
        /// https://github.com/torvalds/linux/blob/master/include/uapi/linux/if_tun.h
        /// </summary>
        internal static uint TUNSETPERSIST = IO._IOW('T', 203, typeof(int));

        public FileDescriptor TunFileDescriptor { get; private set; }

        protected string _tunName;

        /// <summary>
        /// https://www.kernel.org/doc/html/latest/networking/tuntap.html
        /// </summary>
        /// <param name="name">device name</param>
        public Tun(string name)
        {
            TunFileDescriptor = IO.Open("/dev/net/tun", IO.O_RDWR);
            var req = new InterfaceRequestWithFlags();
            req.ifrn_name = name;
            req.ifru_flags = IFF_TUN;
            IO.IOCtl(TunFileDescriptor, TUNSETIFF, ref req);
            _tunName = req.ifrn_name;
        }

        public int Write(byte[] data)
        {
            return IO.Write(TunFileDescriptor, data, data.Length);
        }

        public byte[] Read(int bufferSize)
        {
            var buffer = new byte[bufferSize];
            var readCount = IO.Read(TunFileDescriptor, buffer, bufferSize);
            return buffer.Take(readCount).ToArray();
        }

        public void SetIP(IPAddress address)
        {
            SetIP(address.ToString());
        }

        public void SetIP(string address)
        {
            InterfaceHelper.SetInterfaceAddress(_tunName, address, isMask: false);
        }

        public void SetMask(string address)
        {
            InterfaceHelper.SetInterfaceAddress(_tunName, address, isMask: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    TunFileDescriptor.Dispose();
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~Tun()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
