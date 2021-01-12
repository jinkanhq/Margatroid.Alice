using System.Runtime.InteropServices;

namespace Margatroid.Alice.Native
{
    /// <summary>
    /// https://github.com/torvalds/linux/blob/master/include/uapi/linux/time.h
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct TimeVal
    {
        public int tv_sec;
        public int tv_usec;
    }
}
