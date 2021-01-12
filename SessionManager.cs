using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Margatroid.Alice
{
    class SessionManager
    {
        private List<Session> Sessions = new List<Session>();
        private Dictionary<Guid, Session> GuidSessionIndex
            = new Dictionary<Guid, Session>();
        private Dictionary<IPAddress, Session> VirtualAddressSessionIndex
            = new Dictionary<IPAddress, Session>();

        private List<IPAddress> VirtualIPAddressPool = new List<IPAddress>();

        public SessionManager()
        {
            VirtualIPAddressPool = Enumerable.Range(1, 100).Select(
                end => IPAddress.Parse(string.Format("10.42.42.{0}", end))).ToList();
        }

        public Session Register(IPEndPoint remote, IPAddress localAddress)
        {
            var virtualAddress = pickIPAddress();
            var session = new Session
            {
                TunnelEndPoint = remote,
                VirtualAddress = virtualAddress,
                LocalAddress = localAddress
            };
            Sessions.Add(session);
            GuidSessionIndex.Add(session.Id, session);
            VirtualAddressSessionIndex.Add(virtualAddress, session);
            return session;
        }

        public IPAddress pickIPAddress()
        {
            var avaliableIPAddress = VirtualIPAddressPool[0];
            VirtualIPAddressPool.RemoveAt(0);
            return avaliableIPAddress;
        }

        public bool TryGet(IPAddress virtualAddress, out Session session)
            => VirtualAddressSessionIndex.TryGetValue(virtualAddress, out session);

        public bool TryGet(Guid id, out Session session)
            => GuidSessionIndex.TryGetValue(id, out session);


        public void Delete(Session session)
        {
            GuidSessionIndex.Remove(session.Id);
            VirtualAddressSessionIndex.Remove(session.VirtualAddress);
            VirtualIPAddressPool.Add(session.VirtualAddress);
            Sessions.Remove(session);
        }

        public void Delete(IPAddress virtualAddress)
        {
            Session sessionToDelete;
            if (TryGet(virtualAddress, out sessionToDelete))
            {
                Delete(sessionToDelete);
            }
        }

        public void Delete(Guid id)
        {
            Session sessionToDelete;
            if (TryGet(id, out sessionToDelete))
            {
                Delete(sessionToDelete);
            }
        }
    }
}
