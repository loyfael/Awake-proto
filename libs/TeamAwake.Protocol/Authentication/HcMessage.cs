using TeamAwake.Core.Network.Metadatas;

namespace TeamAwake.Protocol.Authentication;

public sealed class HcMessage : WakfuMessage
{
    public new const string MessageIdentifier = "HC";

    protected override string Identifier =>
        MessageIdentifier;

    public string Key { get; set; }

    public HcMessage(string key) =>
        Key = key;
    
    protected override string Serialize() =>
        string.Concat(Identifier, Key);

    protected override void Deserialize(string message) =>
        Key = message[Identifier.Length..];
}