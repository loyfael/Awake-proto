using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TeamAwake.Core.Network.Metadatas;

namespace TeamAwake.Core.Network.Factories;

/// <summary>
///     A factory abstraction for a component that can create <see cref="WakfuMessage" /> instances with
///     custom configuration for a logical <see cref="WakfuMessage.Identifier"/> value.
/// </summary>
public interface IMessageFactory
{
    /// <summary>Register all messages presents in <paramref name="assembly" />.</summary>
    /// <param name="assembly">The assemblies to find messages of type <see cref="WakfuMessage" />.</param>
    void RegisterMessages(Assembly assembly);

    /// <summary>Gets the name of the message based on its <paramref name="id" />.</summary>
    /// <param name="id">The id of the message.</param>
    /// <returns>The name of the message.</returns>
    /// <remarks>If the <paramref name="id" /> is not valid, returns a <see cref="string.Empty" /> string.</remarks>
    string GetMessageName(string id);

    /// <summary>Attempts to get a network message based on its <paramref name="id" />.</summary>
    /// <param name="id">The id of the message.</param>
    /// <param name="message">The find message.</param>
    /// <returns>Whether the message is null or not.</returns>
    bool TryGetMessage(string id, [NotNullWhen(true)] out WakfuMessage? message);
}