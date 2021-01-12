using System;
using ProtoBuf;

namespace Margatroid.Alice
{
    [ProtoContract]
    public class Message
    {
        [ProtoMember(1, IsRequired = true)]
        public MessageType Type { get; set; }

        [ProtoMember(2)]
        public long Serial { get; set; }

        [ProtoMember(3)]
        public Guid SessionId { get; set; }

        [ProtoMember(4)]
        public SessionType SessionType { get; set; }

        [ProtoMember(5)]
        public byte[] LocalAddress { get; set; }

        [ProtoMember(6)]
        public byte[] VirtualAddress { get; set; }

        [ProtoMember(7)]
        public byte[] TunnelAddress { get; set; }

        [ProtoMember(8)]
        public int TunnelPort { get; set; }

        [ProtoMember(9)]
        public byte[] Payload { get; set; }
    }
}
