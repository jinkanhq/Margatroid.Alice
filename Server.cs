using Margatroid.Alice.Native;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sentry;

using static Margatroid.Alice.MessageHelper;

namespace Margatroid.Alice
{
    class Server : IHostedService, IDisposable
    {
        private bool DisposedValue;
        private CancellationTokenSource CancelSource;

        private readonly ILogger<Server> _logger;
        private readonly IHub _hub;
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _applicationLifetime;

        private UdpClient UdpClient;
        private SessionManager SessionManager;

        public Server(IHub hub, IConfiguration configuration, ILogger<Server> logger, IHostApplicationLifetime appLifetime)
        {
            _hub = hub;
            _configuration = configuration;
            _logger = logger;
            CancelSource = new CancellationTokenSource();
            SessionManager = new SessionManager();
            _applicationLifetime = appLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var bindAddress = IPAddress.Parse(_configuration["Server:ListenAddress"]);
            var bindPort = int.Parse(_configuration["Server:ListenPort"]);
            var bindEndPoint = new IPEndPoint(bindAddress, bindPort);
            UdpClient = new UdpClient(bindEndPoint);

            _logger.LogInformation("Start listening on {bindAddress}:{bindPort}", bindAddress, bindPort);
            _hub.AddBreadcrumb("Breadcrumb added directly to Sentry Hub");

            var mainTask = Task.Run(async () =>
            {
                try
                {
                    await MainLoop();
                }
                catch (Exception e)
                {
                    _hub.CaptureException(e);
                    _logger.LogError(e.ToString());
                    CancelSource.Cancel();
                    _applicationLifetime.StopApplication();
                }
            }, CancelSource.Token);

            await (mainTask.IsCompleted ? mainTask : Task.CompletedTask);
        }

        public async Task MainLoop()
        {
            while (true)
            {
                var receivedResults = await UdpClient.ReceiveAsync();
                var streamBuffer = new MemoryStream(receivedResults.Buffer);
                var message = Serializer.Deserialize<Message>(streamBuffer);

                _logger.LogInformation($"Received message Type={message.Type}");
                if (message.Type != MessageType.Register)
                {
                    Session currentSession;
                    var isSessionExists = SessionManager.TryGet(message.SessionId, out currentSession);
                    if (isSessionExists)
                    {
                        _logger.LogInformation("Session {sessionId} expired", message.SessionId);
                        await SendMessage(UdpClient, new Message { Type = MessageType.SessionExpired }, receivedResults.RemoteEndPoint);
                    }
                }

                switch (message.Type)
                {
                    case MessageType.Register:
                        await Register(message, receivedResults.RemoteEndPoint);
                        continue;
                    case MessageType.HeartBeat:
                        continue;
                    case MessageType.SessionQuery:
                        await SessionQuery(message, receivedResults.RemoteEndPoint);
                        continue;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancelSource.Cancel();
            _logger.LogInformation("4. StopAsync has been called.");
            return Task.CompletedTask;
        }

        public async Task Register(Message message, IPEndPoint remote)
        {
            
            var remoteLocalAddress = new IPAddress(message.LocalAddress);
            var session = SessionManager.Register(remote, remoteLocalAddress);

            SessionType sessionType;
            if (remote.Address.Equals(remoteLocalAddress))
            {
                
                sessionType = SessionType.Public;
            } else if (InterfaceHelper.IsAddressInNetwork(remote.Address, "100.64.0.0/10"))
            {
                sessionType = SessionType.CGNAT;
            } else
            {
                sessionType = SessionType.NAT;
            }

            
            _logger.LogInformation("Session registried. Type={sessionType},LA={remoteLocalAddress},VA={virtualAddress},RA={remoteAddress},RP={remotePort}",
                sessionType, remoteLocalAddress, session.VirtualAddress, remote.Address, remote.Port);


            var successMessage = new Message {
                Type = MessageType.RegisterSuccess,
                SessionId = session.Id,
                SessionType = sessionType,
                VirtualAddress = session.VirtualAddress.GetAddressBytes()
            };
            await SendMessage(UdpClient, successMessage, remote);
            
        }

        public async Task SessionQuery(Message message, IPEndPoint remote)
        {
            Session targetSession;
            var isSessionExists = SessionManager.TryGet(
                new IPAddress(message.VirtualAddress), out targetSession);
            if ( isSessionExists )
            {
                var successMessage = new Message
                {
                    Type = MessageType.SessionExists,
                    VirtualAddress = message.VirtualAddress,
                    TunnelAddress = targetSession.TunnelEndPoint.Address.GetAddressBytes(),
                    TunnelPort = targetSession.TunnelEndPoint.Port
                };
                _logger.LogInformation("Session exists. VA={virtualAddress}", targetSession.VirtualAddress);
                await SendMessage(UdpClient, successMessage, remote);
            } else
            {
                var notFoundMessage = new Message{
                    Type = MessageType.SessionNotFound,
                    VirtualAddress = message.VirtualAddress
                };
                _logger.LogInformation("Session not found. VA={virtualAddress}", targetSession.VirtualAddress);
                await SendMessage(UdpClient, notFoundMessage, remote);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    UdpClient.Dispose();
                    CancelSource.Cancel();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                DisposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Server()
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