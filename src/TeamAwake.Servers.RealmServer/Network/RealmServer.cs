using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamAwake.Core.Cryptography;
using TeamAwake.Core.Network.Dispatchers;
using TeamAwake.Core.Network.Factories;
using TeamAwake.Core.Network.Options;
using TeamAwake.Core.Network.Parsers;
using TeamAwake.Core.Network.Transport;
using TeamAwake.Protocol.Authentication;

namespace TeamAwake.Servers.RealmServer.Network;

public sealed class RealmServer : AbstractServer<RealmSession>
{
    private string? _key;

    public string Key =>
        _key ??= KeyGenerator.GenerateKey();
    
    public RealmServer(
        IServiceProvider provider, 
        IMessageFactory messageFactory, 
        IMessageDispatcher messageDispatcher,
        ILogger<RealmServer> logger,  
        IOptions<TransportServerOptions> options) : base(provider, messageFactory, messageDispatcher, logger, options) { }

    protected override RealmSession CreateSession(
        IMessageParser messageParser, 
        IMessageFactory messageFactory, 
        IMessageDispatcher messageDispatcher, 
        TcpClient client, 
        TransportServerOptions options, 
        ILogger logger) => new(messageParser, messageFactory, messageDispatcher, options, client, logger);

    protected override async Task OnSessionConnectedAsync(RealmSession session)
    {
        await session.SendAsync(new HcMessage(Key));
        
        await base.OnSessionConnectedAsync(session);
    }
}