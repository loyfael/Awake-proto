using System.Buffers;
using System.Text;
using TeamAwake.Core.Network.Factories;
using TeamAwake.Core.Network.Metadatas;
using TeamAwake.Core.Network.Transport;

namespace TeamAwake.Core.Network.Parsers;

/// <inheritdoc />
public sealed class MessageParser : IMessageParser
{
    private readonly IMessageFactory _messageFactory;
    
    public MessageParser(IMessageFactory messageFactory) =>
        _messageFactory = messageFactory;

    /// <inheritdoc />
    public IEnumerable<WakfuMessage> DecodeMessages(ReadOnlySequence<byte> buffer, AbstractSession session)
    {
        if (buffer.IsEmpty)
            return Enumerable.Empty<WakfuMessage>();

        var messages = new List<WakfuMessage>();

        var messagesAsString = Encoding.UTF8.GetString(buffer);

        if (messagesAsString.EndsWith("\nAf\n") && _messageFactory.TryGetMessage("HA", out var identificationMessage))
        {
            identificationMessage.Deserialize(messagesAsString[..^6]);
            return new[] { identificationMessage };
        }

        foreach (var message in messagesAsString.Split('\n'))
        {
            if (string.IsNullOrEmpty(message))
                continue;

            var identifier = message[0] switch
            {
                'A' => message[1] switch
                {
                    'A' => "AA",
                    'D' => "AD",
                    'f' => "Af",
                    'L' => "AL",
                    'S' => "AS",
                    'P' => "AP",
                    _ => "A"
                },
                
                'B' => message[1] switch
                {
                    'A' => "BA",
                    'D' => "BD",
                    'M' => "BM",
                    'W' => "BW",
                    'S' => "BS",
                    'Y' => "BY",
                    _ => "B"
                },
                
                'c' => message[1] switch
                {
                    'C' => message[2] switch
                    {
                        '+' => "cC+",
                        '-' => "cC-",
                        _ => "cC"
                    },
                    _ => "c"
                },
                
                'D' => message[1] switch
                {
                    'C' => "DC",
                    'R' => "DR",
                    'V' => "DV",
                    _ => "D"
                },
                
                'e' => message[1] switch
                {
                    'U' => "eU",
                    _ => "e"
                },
                
                'f' => message[1] switch
                {
                    'D' => "fD",
                    'N' => "fN",
                    'H' => "fH",
                    'L' => "fL",
                    _ => "f"
                },
                
                'G' => message[1] switch
                {
                    'A' => "GA",
                    'C' => "GC",
                    'K' => "GK",
                    'f' => "Gf",
                    'I' => "GI",
                    'p' => "Gp",
                    'R' => "GR",
                    't' => "Gt",
                    'Q' => "GQ",
                    _ => "G"
                },
                
                'w' => message[1] switch
                {
                    'T' => message[2] switch
                    {
                        'L' => "wTL",
                        'C' => "wTC",
                        _ => "wT"
                    },
                    'C' => "wC",
                    _ => "w"
                },
                
                'O' => message[1] switch
                {
                    'M' => "OM",
                    'D' => "OD",
                    'd' => "Od",
                    _ => "O"
                },
                
                'S' => message[1] switch
                {
                    'B' => "SB",
                    'F' => "SF",
                    'M' => "SM",
                    _ => "S"
                },
                
                _ => message[0].ToString()
            };
            
            if (!_messageFactory.TryGetMessage(identifier, out var wakfuMessage))
                continue;
            
            wakfuMessage.Deserialize(message[identifier.Length..]);
            
            messages.Add(wakfuMessage);
        }

        return messages;
    }

    /// <inheritdoc />
    public ReadOnlyMemory<byte> EncodeMessage(WakfuMessage message) =>
        Encoding.UTF8.GetBytes(string.Concat(message.Serialize(), "\n\0"));
}