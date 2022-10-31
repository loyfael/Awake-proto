using System.Net;
using TeamAwake.Core.Network.Transport;

namespace TeamAwake.Core.Network.Options;

/// <summary>Describes a configuration for <see cref="AbstractServer{TSession}" />.</summary>
public sealed class TransportServerOptions
{
    /// <summary>Gets the ip address.</summary>
    public required string IpAddress { get; set; }

    /// <summary>Gets the port.</summary>
    public required int Port { get; set; }

    /// <summary>Gets the number of max connections pending.</summary>
    public int MaxConnections { get; set; }

    /// <summary>Gets the number of max connections pending by ip address.</summary>
    public int MaxConnectionsByIpAddress { get; set; }

    /// <summary>Indicates whether server session log receives and sends.</summary>
    public bool LogMessages { get; set; }

    /// <summary>Creates a new <see cref="IPEndPoint" /> from the <see cref="IpAddress" /> and <see cref="Port" />.</summary>
    /// <returns>A new instance of <see cref="IPEndPoint" />.</returns>
    internal IPEndPoint CreateEndPoint() =>
        new(IPAddress.Parse(IpAddress), Port);
}