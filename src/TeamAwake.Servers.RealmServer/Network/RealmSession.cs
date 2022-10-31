using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TeamAwake.Core.Network.Dispatchers;
using TeamAwake.Core.Network.Factories;
using TeamAwake.Core.Network.Options;
using TeamAwake.Core.Network.Parsers;
using TeamAwake.Core.Network.Transport;

namespace TeamAwake.Servers.RealmServer.Network;

public sealed class RealmSession : AbstractSession
{
    public RealmSession(
        IMessageParser messageParser, 
        IMessageFactory messageFactory, 
        IMessageDispatcher messageDispatcher, 
        TransportServerOptions serverOptions, 
        TcpClient client, 
        ILogger logger) : base(messageParser, messageFactory, messageDispatcher, serverOptions, client, logger) { }
}