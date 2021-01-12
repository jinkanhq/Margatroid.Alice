using System;
using System.Net;

namespace Margatroid.Alice
{
    class Session
    {
        public Guid Id { get; private set; }

        public SessionType type {
            get {
                if (TunnelEndPoint.Address == LocalAddress)
                {
                    return SessionType.Public;
                }
                return SessionType.NAT;
            }
        }

        public IPEndPoint TunnelEndPoint { get; set; }

        public IPAddress VirtualAddress { get; set; }

        public IPAddress LocalAddress { get; set; }

        public Session()
        {
            Id = Guid.NewGuid();
        }
    }
}
