using TeamAwake.Core.Network.Metadatas;

namespace TeamAwake.Protocol.Security;

public sealed class IdentificationMessage : WakfuMessage
{
    public new const string MessageIdentifier = "HA";
    
    public string Version { get; set; }
    
    public string Username { get; set; }
    
    public string Password { get; set; }

    protected override string Identifier =>
        MessageIdentifier;

    protected override string Serialize() =>
        string.Empty;

    protected override void Deserialize(string message)
    {
        var parts = message.Split('\n');

        Version = parts[0];
        Username = parts[1];
        Password = parts[2];
    }
}