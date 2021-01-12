using System;
using System.Runtime.InteropServices;

namespace Margatroid.Alice.Native
{
    internal static class IO
	{
		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/asm-generic/fcntl.h
		/// </summary>
		internal const int O_RDONLY = 00000000;
		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/asm-generic/fcntl.h
		/// </summary>
		internal const int O_WRONLY = 00000001;
		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/asm-generic/fcntl.h
		/// </summary>
		internal const int O_RDWR = 00000002;
		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/asm-generic/fcntl.h
		/// </summary>
		internal const int O_NONBLOCK = 00004000;
		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/asm-generic/fcntl.h
		/// get file->f_flags
		/// </summary>
		internal const int F_GETFL = 3;
		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/asm-generic/fcntl.h
		/// set file->f_flags
		/// </summary>
		internal const int F_SETFL = 4;


		internal static ushort AF_INET = 2;
		internal static ushort AF_INET6 = 10;

		internal static ushort IPPROTO_TCP = 6;
		internal static ushort IPPROTO_UDP = 16;

		internal static ushort SOCK_STREAM = 1;
		internal static ushort SOCK_DGRAM = 2;

		internal static int EPOLL_CTL_ADD = 1;
		internal static int EPOLL_CTL_DEL = 2;
		internal static int EPOLL_CTL_MOD = 3;

		/// <summary>
		/// https://github.com/torvalds/linux/blob/d635a69dd4981cc51f90293f5f64268620ed1565/include/uapi/asm-generic/socket.h
		/// </summary>
		internal static int SOL_SOCKET = 1;

		/// <summary>
		/// https://github.com/torvalds/linux/blob/d635a69dd4981cc51f90293f5f64268620ed1565/include/uapi/asm-generic/socket.h
		/// </summary>
		internal static int SO_REUSEADDR = 2;

		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/linux/in.h
		/// Dummy protocol for TCP
		/// </summary>
		internal static ushort IPPROTO_IP = 0;

		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/linux/sockios.h
		/// set flags
		/// </summary>
		internal static uint SIOCSIFFLAGS = 0x8914;

		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/linux/sockios.h
		/// get PA address
		/// </summary>
		internal static uint SIOCGIFADDR = 0x8915;

		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/linux/sockios.h
		/// set PA address
		/// </summary>
		internal static uint SIOCSIFADDR = 0x8916;

		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/linux/sockios.h
		/// set network PA mask
		/// </summary>
		internal static uint SIOCSIFNETMASK = 0x891c;

		/// <summary>
		/// https://github.com/torvalds/linux/blob/master/include/uapi/linux/sockios.h
		/// set MTU size
		/// </summary>
		internal static uint SIOCSIFMTU = 0x8922;

		internal static int Check(int returnValue)
		{
			if (returnValue == -1)
			{
				throw new NativeException(Marshal.GetLastWin32Error());
			}
			return returnValue;
		}

		internal static FileDescriptor Check(FileDescriptor fd)
		{
			if (fd.IsInvalid)
			{
				throw new NativeException(Marshal.GetLastWin32Error());
			}
			return fd;
		}

		[DllImport("libc", EntryPoint = "open", SetLastError = true)]
		internal static extern FileDescriptor Open(string fileName, int mode);
		 
		[DllImport("libc", EntryPoint = "close", SetLastError = true)]
		internal static extern int Close(int fd);

		[DllImport("libc", EntryPoint = "read", SetLastError = true)]
		internal static extern int Read(FileDescriptor fd, [In, Out] byte[] buf, int count);

		[DllImport("libc", EntryPoint = "read", SetLastError = true)]
		internal static extern int Read(int fd, [In, Out] byte[] buf, int count);

		[DllImport("libc", EntryPoint = "write", SetLastError = true)]
		internal static extern int Write(FileDescriptor fd, byte[] buf, int count);

		[DllImport("libc", EntryPoint = "fcntl", SetLastError = true)]
		internal static extern int Fcntl(FileDescriptor fd, int cmd, int arg);

		[DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
		internal static extern int IOCtl(FileDescriptor fd, uint request, ref InterfaceRequestWithFlags data);

		[DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
		internal static extern int IOCtl(FileDescriptor fd, uint request, ref InterfaceRequestWithAddress data);

		[DllImport("libc", EntryPoint = "inet_pton", SetLastError = true)]
		internal static extern int Inet_pton(int af, string src, ref uint dst);

		[DllImport("libc", EntryPoint = "socket", SetLastError = true)]
		internal static extern FileDescriptor Socket(int domain, int type, int protocol);

		[DllImport("libc", EntryPoint = "setsockopt", SetLastError = true)]
		internal static extern FileDescriptor SetSockOpt(FileDescriptor socket, int level, int option_name, ref int option_value, int option_len = 1);

        [DllImport("libc", EntryPoint = "bind", SetLastError = true)]
        internal static extern int Bind(FileDescriptor sockfd, ref SockAddr addr, int addrlen);

        [DllImport("libc", EntryPoint = "recvfrom", SetLastError = true)]
		internal static extern int RecvFrom(FileDescriptor sockfd, byte[] buf, int len, int flags, ref SockAddr dest_addr, ref int addrlen);

		[DllImport("libc", EntryPoint = "sendto", SetLastError = true)]
		internal static extern int SendTo(FileDescriptor sockfd, byte[] buf, int len, int flags, ref SockAddr dest_addr, int addrlen);

		[DllImport("libc", EntryPoint = "select", SetLastError = true)]
		internal static extern int Select(int nfds, FileDescriptor[] readfds, FileDescriptor[] writefds, FileDescriptor[] exceptfds, TimeVal timeout);

		[DllImport("libc", EntryPoint = "epoll_create1", SetLastError = true)]
		internal static extern FileDescriptor EpollCreate(int flags = 0);

		[DllImport("libc", EntryPoint = "epoll_ctl", SetLastError = true)]
		internal static extern int EpollCtl(FileDescriptor epfd, int op, FileDescriptor fd, ref EpollEvent epollEvent);

		[DllImport("libc", EntryPoint = "epoll_wait", SetLastError = true)]
		internal static extern int EpollWait(FileDescriptor epfd, [In, Out] EpollEvent[] epollEvent, int maxevents, int timeout);



		/*
		 * https://github.com/torvalds/linux/blob/master/include/uapi/asm-generic/ioctl.h
		 * 
		 * The following is for compatibility across the various Linux
		 * platforms.  The generic ioctl numbering scheme doesn't really enforce
		 * a type field.  De facto, however, the top 8 bits of the lower 16
		 * bits are indeed used as a type field, so we might just as well make
		 * this explicit here.  Please be sure to use the decoding macros
		 * below from now on.
		 */
		internal const int _IOC_NRBITS = 8;
		internal const int _IOC_TYPEBITS = 8;

		/*
		 * Let any architecture override either of the following before
		 * including this file.
		 */
		internal const int _IOC_SIZEBITS = 14;
		internal const int _IOC_DIRBITS = 2;

		internal const int _IOC_NRMASK = (1 << _IOC_NRBITS) - 1;
		internal const int _IOC_TYPEMASK = (1 << _IOC_TYPEBITS) - 1;
		internal const int _IOC_SIZEMASK = (1 << _IOC_SIZEBITS) - 1;
		internal const int _IOC_DIRMASK = (1 << _IOC_DIRBITS) - 1;

		internal const int _IOC_NRSHIFT = 0;
		internal const int _IOC_TYPESHIFT = _IOC_NRSHIFT + _IOC_NRBITS;
		internal const int _IOC_SIZESHIFT = _IOC_TYPESHIFT + _IOC_TYPEBITS;
		internal const int _IOC_DIRSHIFT = _IOC_SIZESHIFT + _IOC_SIZEBITS;

		/*
		 * Direction bits, which any architecture can choose to override
		 * before including this file.
		 *
		 * NOTE: _IOC_WRITE means userland is writing and kernel is
		 * reading. _IOC_READ means userland is reading and kernel is writing.
		 */
		internal const uint _IOC_NONE = 0U;
		internal const uint _IOC_WRITE = 1U;
		internal const uint _IOC_READ = 2U;

		internal static uint _IOC(uint dir, int type, int nr, uint size) =>
            (dir << _IOC_DIRSHIFT) | (uint)(type << _IOC_TYPESHIFT) | (uint)(nr << _IOC_NRSHIFT) | (size << _IOC_SIZESHIFT);

		internal static uint _IOC_TYPECHECK(Type t)
        {
			if (t == typeof(int)) return sizeof(int);
			if (t == typeof(uint)) return sizeof(uint);
			return 0;
        }

        /*
		 * Used to create numbers.
		 *
		 * NOTE: _IOW means userland is writing and kernel is reading. _IOR
		 * means userland is reading and kernel is writing.
		 */
        internal static uint _IO(int type, int nr) => _IOC(_IOC_NONE, type, nr, 0);

		internal static uint _IOR(int type, int nr, Type size) => _IOC(_IOC_READ, type, nr, _IOC_TYPECHECK(size));

		internal static uint _IOW(int type, int nr, Type size) => _IOC(_IOC_WRITE, type, nr, _IOC_TYPECHECK(size));

		internal static uint _IOWR(int type, int nr, Type size) => _IOC(_IOC_READ | _IOC_WRITE, type, nr, _IOC_TYPECHECK(size));

		internal static uint _IOR_BAD(int type, int nr, Type size) => _IOC(_IOC_READ, type, nr, _IOC_TYPECHECK(size));

		internal static uint _IOW_BAD(int type, int nr, Type size) => _IOC(_IOC_WRITE, type, nr, _IOC_TYPECHECK(size));

		internal static uint _IOWR_BAD(int type, int nr, Type size) => _IOC(_IOC_READ | _IOC_WRITE, type, nr, _IOC_TYPECHECK(size));

		/* used to decode ioctl numbers.. */
		internal static int _IOC_DIR(int nr) => ((nr) >> _IOC_DIRSHIFT) & _IOC_DIRMASK;
		internal static int _IOC_TYPE(int nr) => ((nr) >> _IOC_TYPESHIFT) & _IOC_TYPEMASK;
		internal static int _IOC_NR(int nr) => ((nr) >> _IOC_NRSHIFT) & _IOC_NRMASK;
		internal static int _IOC_SIZE(int nr) => ((nr) >> _IOC_SIZESHIFT) & _IOC_SIZEMASK;

		/* ...and for the drivers/sound files... */
		internal const uint IOC_IN = _IOC_WRITE << _IOC_DIRSHIFT;

		internal const uint IOC_OUT = _IOC_READ << _IOC_DIRSHIFT;

		internal const uint IOC_INOUT = (_IOC_WRITE | _IOC_READ) << _IOC_DIRSHIFT;

		internal const int IOCSIZE_MASK = _IOC_SIZEMASK << _IOC_SIZESHIFT;

		internal const int IOCSIZE_SHIFT = _IOC_SIZESHIFT;
	}
}
