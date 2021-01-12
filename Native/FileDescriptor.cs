using System;
using System.Runtime.InteropServices;

namespace Margatroid.Alice.Native
{
    public sealed class FileDescriptor : SafeHandle
    {

        private FileDescriptor(): base(new IntPtr(-1), true)
        {
        }

        public static bool operator ==(int left, FileDescriptor right)
        {
            return left == right.DangerousGetHandle().ToInt64();
        }

        public static bool operator !=(int left, FileDescriptor right)
        {
            return left != right.DangerousGetHandle().ToInt64();
        }

        public static bool operator ==(FileDescriptor left, int right)
        {
            return left.DangerousGetHandle().ToInt64() == right;
        }

        public static bool operator !=(FileDescriptor left, int right)
        {
            return left.DangerousGetHandle().ToInt64() != right;
        }

        public static bool operator ==(FileDescriptor left, FileDescriptor right)
        {
            return left.DangerousGetHandle() == right.DangerousGetHandle();
        }

        public static bool operator !=(FileDescriptor left, FileDescriptor right)
        {
            return left.DangerousGetHandle() != right.DangerousGetHandle();
        }

        public override bool IsInvalid => handle.ToInt64() == ((long)1 << 32) - 1;

        override protected bool ReleaseHandle()
        {
            var returnValue = IO.Close((int)handle.ToInt64());
            if (returnValue == -1)
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
