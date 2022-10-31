using TeamAwake.Core.Network.Dispatchers;
using TeamAwake.Protocol.Security;
using TeamAwake.Servers.RealmServer.Network;

namespace TeamAwake.Servers.RealmServer.Handlers.Authentication;

public sealed class AuthenticationHandler
{
    private const string WakfuVersion = "1.12.0s";
    
    [Message]
    public async Task HandleHandshakeAsync(IdentificationMessage message, RealmSession session)
    {
        if (!message.Version.Equals(WakfuVersion, StringComparison.InvariantCultureIgnoreCase))
        {
            session.Disconnect();
            return;
        }
    }
}

// GM s'occupe de la gestion des maps