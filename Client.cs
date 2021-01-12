using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Margatroid.Alice.Native;
using static Margatroid.Alice.MessageHelper;
using static Margatroid.Alice.Native.IO;

namespace Margatroid.Alice
{
    class Client : IHostedService, IDisposable
    {
        private const int BufferSize = 8192;
        private bool DisposedValue;
        private readonly CancellationTokenSource CancelSource;

        private readonly ILogger<Server> _logger;
        private readonly IHub _hub;
        private readonly IConfiguration _configuration;

        private Guid SessionId;
        private readonly Dictionary<IPAddress, IPEndPoint> Tunnels;
        private readonly HashSet<IPAddress> QueriedVirtualAddresses;
        private readonly IPEndPoint ServerEndPoint;
        private readonly IPEndPoint ListenEndPoint;
        private readonly Queue<TunFrame> TunnelSendQueue;
        private readonly Queue<TunFrame> TunnelReceiveQueue;

        private readonly Tun Tun;
        private readonly UdpSocket UdpSocket;

        public Client(IHub hub, IConfiguration configuration, ILogger<Server> logger, IHostApplicationLifetime appLifetime)
        {
            _hub = hub;
            _configuration = configuration;
            _logger = logger;
            CancelSource = new CancellationTokenSource();
            QueriedVirtualAddresses = new HashSet<IPAddress>();
            Tunnels = new Dictionary<IPAddress, IPEndPoint>();
            Tun = new Tun("alicetun");
            var serverAddress = IPAddress.Parse(_configuration["Client:ServerAddress"]);
            var serverPort = int.Parse(_configuration["Client:ServerPort"]);
            ServerEndPoint = new IPEndPoint(serverAddress, serverPort);
            var listenAddress = IPAddress.Parse(_configuration["Client:ListenAddress"]);
            var listenPort = int.Parse(_configuration["Client:ListenPort"]);
            ListenEndPoint = new IPEndPoint(listenAddress, listenPort);
            TunnelReceiveQueue = new Queue<TunFrame>();
            TunnelSendQueue = new Queue<TunFrame>();
            UdpSocket = new UdpSocket();
            UdpSocket.Bind(ListenEndPoint);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var mainTask = Task.Run(() =>
            {
                try
                {
                    MainLoop();
                }
                catch (Exception e)
                {
                    _hub.CaptureException(e);
                    _logger.LogError(e.ToString());
                    CancelSource.Cancel();
                }
            }, CancelSource.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancelSource.Cancel();
            return Task.CompletedTask;
        }

        public void MainLoop()
        {
            // Init socket
            // var socketFileDescriptorFlags = Check(Fcntl(UdpSocket, F_GETFL, 0));
            // Check(Fcntl(socketFileDescriptor, F_SETFL, socketFileDescriptorFlags | O_NONBLOCK));
            _logger.LogInformation("Start listening on {bindAddress}:{bindPort}", ListenEndPoint.Address, ListenEndPoint.Port);

            // Register to server
            Register();

            // Create epoll
            const int maxEvents = 64;
            using var epollFileDescriptor = EpollCreate();

            // Add tun to epoll
            var tunEpollEvent = new EpollEvent();
            tunEpollEvent.events = EpollEvents.EPOLLIN | EpollEvents.EPOLLOUT;
            tunEpollEvent.fd = (int)Tun.TunFileDescriptor.DangerousGetHandle().ToInt64();
            Check(EpollCtl(epollFileDescriptor, EPOLL_CTL_ADD, Tun.TunFileDescriptor, ref tunEpollEvent));
            // Add UDP socket to epoll
            var socketEpollEvent = new EpollEvent();
            socketEpollEvent.events = EpollEvents.EPOLLIN | EpollEvents.EPOLLOUT;
            tunEpollEvent.fd = (int)UdpSocket.SocketFileDescriptor.DangerousGetHandle().ToInt64();
            Check(EpollCtl(epollFileDescriptor, EPOLL_CTL_ADD, UdpSocket.SocketFileDescriptor, ref socketEpollEvent));

            // main loop
            Message message;
            IPEndPoint remote;
            TunFrame tunFrame;
            var epollReceivedEvents = new EpollEvent[maxEvents];
            while (true)
            {
                var epollWaitRet = Check(EpollWait(epollFileDescriptor, epollReceivedEvents, maxEvents, -1));
                for (var i = 0; i < epollWaitRet; i++)
                {
                    var fd = epollReceivedEvents[i].fd;
                    if (epollReceivedEvents[i].events == EpollEvents.EPOLLIN)
                    {
                        if (fd == Tun.TunFileDescriptor)
                        {
                            var data = Tun.Read(BufferSize);
                            TunnelSendQueue.Enqueue(new TunFrame(data));

                        }
                        else if (fd == UdpSocket.SocketFileDescriptor)
                        {
                            var data = UdpSocket.Recv(BufferSize, out remote);
                            message = DeserializeMessage(data);
                            ListenDispatch(message, remote);
                        }
                        else
                        {
                            throw new AliceException("Unexpected file descriptor");
                        }
                    }
                    else if (epollReceivedEvents[i].events == EpollEvents.EPOLLOUT)
                    {
                        if (fd == Tun.TunFileDescriptor)
                        {
                            if (TunnelReceiveQueue.TryDequeue(out tunFrame))
                            {
                                Tun.Write(tunFrame.FrameData);
                            }
                        }
                        else if (fd == UdpSocket.SocketFileDescriptor)
                        {
                            if (TunnelSendQueue.TryDequeue(out tunFrame))
                            {
                                SendTunnel(tunFrame);
                            }
                        }
                    }
                }
            }
        }

        public void Register()
        {
            var registerMessage = new Message
            {
                Type = MessageType.Register,
                LocalAddress = InterfaceHelper.GetLocalAddress().GetAddressBytes()
            };
            _logger.LogInformation($"Registering to {ServerEndPoint.Address}");
            UdpSocket.SendMessageTo(ServerEndPoint, registerMessage);
            IPEndPoint remote;
            var message = UdpSocket.RecvMessage(out remote);
            var virtualIPAddress = new IPAddress(message.VirtualAddress);
            _logger.LogInformation("MessageType={type}, VA={virtualAddress}", message.Type, virtualIPAddress);

            if (message.Type != MessageType.RegisterSuccess)
            {
                throw new AliceException("Cannot register on server");
            }
            SessionId = message.SessionId;
            Tun.SetIP(virtualIPAddress);
            Tun.SetMask("255.255.255.0");
        }

        public void ListenDispatch(Message message, IPEndPoint remote)
        {
            switch (message.Type)
            {
                case MessageType.HeartBeat:
                    HeartBeat();
                    return;
                case MessageType.Tunnel:
                    TunnelReceiveQueue.Enqueue(new TunFrame(message.Payload));
                    return;
                case MessageType.SessionBroadcast:
                    return;
                case MessageType.SessionExists:
                    var virtualAddress = new IPAddress(message.VirtualAddress);
                    var tunnelAddress = new IPAddress(message.TunnelAddress);
                    var tunnelEndPoint = new IPEndPoint(tunnelAddress, message.TunnelPort);
                    _logger.LogInformation($"Session exists. VA={new IPAddress(message.VirtualAddress)} "
                        + $"TA={tunnelAddress} "
                        + $"TP={message.TunnelPort}");
                    Tunnels.Add(virtualAddress, tunnelEndPoint);
                    return;
                case MessageType.SessionNotFound:
                    _logger.LogInformation($"Session not found. VA={new IPAddress(message.VirtualAddress)}");
                    return;
                case MessageType.SessionExpired:
                    Register();
                    QueriedVirtualAddresses.Clear();
                    Tunnels.Clear();
                    TunnelSendQueue.Clear();
                    TunnelReceiveQueue.Clear();
                    return;
            }
        }

        public void HeartBeat()
        {
            var message = new Message
            {
                Type = MessageType.HeartBeat,
                SessionId = SessionId
            };
            UdpSocket.SendMessageTo(ServerEndPoint, message);
        }

        public void SendTunnel(TunFrame tunFrame)
        {
            IPEndPoint remote;
            if (Tunnels.TryGetValue(tunFrame.DestinationAddress, out remote))
            {
                var message = new Message
                {
                    Type = MessageType.Tunnel,
                    Payload = tunFrame.FrameData
                };
                UdpSocket.SendMessageTo(remote, message);
            } else if (!QueriedVirtualAddresses.Contains(tunFrame.DestinationAddress))
            {
                var message = new Message
                {
                    SessionId = SessionId,
                    Type = MessageType.SessionQuery,
                    VirtualAddress = tunFrame.DestinationAddress.GetAddressBytes()
                };
                UdpSocket.SendMessageTo(ServerEndPoint, message);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                UdpSocket.Dispose();
                Tun.Dispose();
                DisposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Client()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}