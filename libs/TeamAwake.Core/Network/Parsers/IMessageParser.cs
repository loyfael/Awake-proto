using System.Buffers;
using TeamAwake.Core.Network.Metadatas;
using TeamAwake.Core.Network.Transport;

namespace TeamAwake.Core.Network.Parsers;

/// <summary>Describe a way to encode or decode a network message.</summary>
public interface IMessageParser
{
    /// <summary>Attempts to parse a network message based on <paramref name="buffer" />.</summary>
    /// <param name="buffer">The buffer containing the data.</param>
    /// <param name="session">The underlying session.</param>
    /// <returns>A collection of <see cref="WakfuMessage"/>.</returns>
    IEnumerable<WakfuMessage> DecodeMessages(ReadOnlySequence<byte> buffer, AbstractSession session);
    
    /// <summary>Encodes the message in an <see cref="ReadOnlyMemory{T}" />.</summary>
    /// <param name="message">The message to encode.</param>
    /// <returns>A <see cref="ReadOnlyMemory{T}" /> containing the encoded message.</returns>
    ReadOnlyMemory<byte> EncodeMessage(WakfuMessage message);
}