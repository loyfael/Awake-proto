using System.Reflection;
using TeamAwake.Core.Network.Metadatas;
using TeamAwake.Core.Network.Transport;

namespace TeamAwake.Core.Network.Dispatchers;

/// <summary>Describe a way to dispatch a network message.</summary>
public interface IMessageDispatcher
{
    /// <summary>Register all handlers presents in <paramref name="assembly" />.</summary>
    /// <param name="assembly">The assembly to find handlers of type <see cref="WakfuMessage" />.</param>
    void RegisterHandlers(Assembly assembly);

    /// <summary>Dispatches a network message asynchronously.</summary>
    /// <param name="message">The message to dispatch.</param>
    /// <param name="session">The session.</param>
    Task<DispatchResults> DispatchMessageAsync(WakfuMessage message, AbstractSession session);
}