using System.Runtime.InteropServices;


namespace Margatroid.Alice.Native
{
    /// <summary>
    /// https://github.com/bminor/glibc/blob/master/sysdeps/unix/sysv/linux/sys/epoll.h
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct EpollEvent
    {
        public EpollEvents events;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        //public byte[] data;
        public int fd;
    }
}
