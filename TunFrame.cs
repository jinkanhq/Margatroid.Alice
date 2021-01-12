using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Margatroid.Alice.Native
{
    class TunFrame
    {
        public readonly byte[] FrameData;

        public readonly int Flag;

        public readonly int Proto;

        public readonly IPAddress SourceAddress;

        public readonly IPAddress DestinationAddress;

        public TunFrame(byte[] data)
        {
            FrameData = data;
            // Flag (2 bytes)
            Flag = BitConverter.ToInt32(new byte[] { FrameData[0], FrameData[1] });
            // Proto (2 bytes)
            Proto = BitConverter.ToInt32(new byte[] { FrameData[2], FrameData[3] });
            // IP Packet
            SourceAddress = new IPAddress(Enumerable.Range(12 + 4, 4).Select(i => FrameData[i]).ToArray());
            DestinationAddress = new IPAddress(Enumerable.Range(15 + 4, 4).Select(i => FrameData[i]).ToArray());
        }
    }
}
