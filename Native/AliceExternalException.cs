using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Margatroid.Alice.Native
{
    class NativeException : Exception
    {

		private static string[] ErrnoMessages = new string[]
		{
			"",
			"Operation not permitted",
			"No such file or directory",
			"No such process",
			"Interrupted system call",
			"I/O error",
			"No such device or address",
			"Argument list too long",
			"Exec format error",
			"Bad file number",
			"No child processes",
			"Try again",
			"Out of memory",
			"Permission denied",
			"Bad address",
			"Block device required",
			"Device or resource busy",
			"File exists",
			"Cross-device link",
			"No such device",
			"Not a directory",
			"Is a directory",
			"Invalid argument",
			"File table overflow",
			"Too many open files",
			"Not a typewriter",
			"Text file busy",
			"File too large",
			"No space left on device",
			"Illegal seek",
			"Read-only file system",
			"Too many links",
			"Broken pipe",
			"Math argument out of domain of func",
			"Math result not representable",
		};

		public Errno Errno { get; }

		public override string Message { get => $"[{Errno}] {ErrnoMessages[(int)Errno]}"; }

		public NativeException(int errno, string message = "") : base(message)
        {
            Errno = (Errno)errno;
        }

        public NativeException(Errno errno, string message = "") : base(message)
		{
            Errno = errno;
        }
    }
}
