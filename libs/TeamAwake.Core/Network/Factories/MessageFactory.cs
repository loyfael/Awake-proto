using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using TeamAwake.Core.Network.Metadatas;

namespace TeamAwake.Core.Network.Factories;

/// <inheritdoc />
public sealed class MessageFactory : IMessageFactory
{
    private readonly ILogger<MessageFactory> _logger;
    private readonly ConcurrentDictionary<string, string> _messageNames;
    private readonly ConcurrentDictionary<string, Func<WakfuMessage>> _messages;

    /// <summary>Initializes a new instance of the <see cref="MessageFactory" /> class.</summary>
    /// <param name="logger">The logger.</param>
    public MessageFactory(ILogger<MessageFactory> logger)
    {
        _messages = new ConcurrentDictionary<string, Func<WakfuMessage>>();
        _messageNames = new ConcurrentDictionary<string, string>();
        _logger = logger;
    }

    /// <inheritdoc />
    public void RegisterMessages(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(WakfuMessage))))
        {
            var messageId = Convert.ToString(type.GetField("MessageIdentifier")?.GetValue(null), CultureInfo.InvariantCulture)!;

            if (string.IsNullOrEmpty(messageId))
                continue;
            
            if (_messages.ContainsKey(messageId))
                throw new InvalidOperationException("A message with the same id already exists.");

            var factory = Expression.Lambda<Func<WakfuMessage>>(Expression.New(type)).Compile();

            _messages.TryAdd(messageId, factory);
            _messageNames.TryAdd(messageId, type.Name);
        }
        
        _logger.LogInformation("{Count} wakfu messages registered", _messages.Count);
    }

    /// <inheritdoc />
    public string GetMessageName(string id) =>
        _messageNames.TryGetValue(id, out var messageName)
            ? messageName
            : throw new InvalidOperationException($"Message with key {id} not found");

    /// <inheritdoc />
    public bool TryGetMessage(string id, [NotNullWhen(true)] out WakfuMessage? message)
    {
        message = null;

        if (!_messages.TryGetValue(id, out var factory))
            return false;

        message = factory();
        return true;
    }
}