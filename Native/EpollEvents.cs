using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Margatroid.Alice.Native
{
    [Flags]
    internal enum EpollEvents : uint
    {
        EPOLLIN = 0x001,

        EPOLLPRI = 0x002,

        EPOLLOUT = 0x004,

        EPOLLRDNORM = 0x040,

        EPOLLRDBAND = 0x080,

        EPOLLWRNORM = 0x100,

        EPOLLWRBAND = 0x200,

        EPOLLMSG = 0x400,

        EPOLLERR = 0x008,

        EPOLLHUP = 0x010,

        EPOLLRDHUP = 0x2000,

        EPOLLEXCLUSIVE = 1u << 28,

        EPOLLWAKEUP = 1u << 29,

        EPOLLONESHOT = 1u << 30,

        EPOLLET = 1u << 31
    }
}
