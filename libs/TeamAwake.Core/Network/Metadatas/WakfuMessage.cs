namespace TeamAwake.Core.Network.Metadatas;

/// <summary>Represents a network tcp message.</summary>
public abstract class WakfuMessage
{
    /// <summary>Gets the message identifier.</summary>
    public const string MessageIdentifier = "";
    
    /// <summary>Gets the message identifier.</summary>
    protected internal abstract string Identifier { get; }
    
    /// <summary>Serializes the content of the underlying message.</summary>
    /// <returns>The serialized message into a <see cref="string"/>.</returns>
    protected internal abstract string Serialize();
    
    /// <summary>Deserializes the content of <paramref name="message"/>.</summary>
    /// <param name="message">The message to deserialize.</param>
    protected internal abstract void Deserialize(string message);
}