using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Margatroid.Alice
{
    class AliceException : Exception
    {
        public AliceException(string message) : base(message)
        {
        }
    }
}
