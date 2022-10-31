using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamAwake.Core.Network.Extensions;
using TeamAwake.Core.Network.Metadatas;
using TeamAwake.Core.Network.Transport;

namespace TeamAwake.Core.Network.Dispatchers;

/// <inheritdoc />
public sealed class MessageDispatcher : IMessageDispatcher
{
    private readonly ConcurrentDictionary<string, (Type Type, Func<object, WakfuMessage, AbstractSession, Task> Lambda)> _handlers;
    private readonly ILogger<MessageDispatcher> _logger;
    private readonly IServiceProvider _provider;

    /// <summary>Initializes a new instance of the <see cref="MessageDispatcher" /> class.</summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public MessageDispatcher(IServiceProvider provider, ILogger<MessageDispatcher> logger)
    {
        _provider = provider;
        _logger = logger;
        _handlers = new ConcurrentDictionary<string, (Type Type, Func<object, WakfuMessage, AbstractSession, Task> Lambda)>();
    }

    /// <inheritdoc />
    public void RegisterHandlers(Assembly assembly)
    {
        foreach (var (type, method) in from type in assembly.GetTypes()
                from method in type.GetMethods()
                let attribute = method.GetCustomAttribute<MessageAttribute>()
                where attribute is not null
                select (type, method))
        {
            var methodParameters = method.GetParameters();

            if (methodParameters.Length != 2)
                throw new InvalidOperationException($"Method {method.Name} in {type.Name} has an invalid number of parameters");

            if (!methodParameters[0].ParameterType.IsAssignableTo(typeof(WakfuMessage)))
                throw new InvalidOperationException($"Method {method.Name} in {type.Name} has an invalid parameter type");

            if (!methodParameters[1].ParameterType.IsAssignableTo(typeof(AbstractSession)))
                throw new InvalidOperationException($"Method {method.Name} in {type.Name} has an invalid parameter type");

            var messageId = Convert.ToString(methodParameters[0].ParameterType.GetField("MessageIdentifier")?.GetValue(null), CultureInfo.InvariantCulture)!;

            if (_handlers.ContainsKey(messageId))
                throw new InvalidOperationException("A handler with the same key is already registered");

            var factory = method.CreateDelegate<WakfuMessage, AbstractSession, Task>();

            _handlers.TryAdd(messageId, (type, factory));
        }

        _logger.LogInformation("{Count} dofus handler registered", _handlers.Count);
    }

    /// <inheritdoc />
    public async Task<DispatchResults> DispatchMessageAsync(WakfuMessage message, AbstractSession session)
    {
        if (!_handlers.TryGetValue(message.Identifier, out var handler))
            return DispatchResults.Unhandled;

        try
        {
            var instance = _provider.GetRequiredService(handler.Type);

            await handler.Lambda(instance, message, session).ConfigureAwait(false);

            return DispatchResults.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while dispatching message {MessageId}", message.Identifier);

            return DispatchResults.Failure;
        }
    }
}