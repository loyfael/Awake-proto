using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamAwake.Core.Network.Dispatchers;
using TeamAwake.Core.Network.Factories;
using TeamAwake.Core.Network.Options;
using TeamAwake.Core.Network.Parsers;

namespace TeamAwake.Core.Network.Transport;

/// <summary>Describes a way for listen and accept sessions asynchronously.</summary>
/// <typeparam name="TSession">The type of the session.</typeparam>
public abstract class AbstractServer<TSession>
    where TSession : AbstractSession
{
    private readonly CancellationTokenSource _cts;
    private readonly IMessageDispatcher _messageDispatcher;
    private readonly IMessageFactory _messageFactory;
    private readonly IServiceProvider _provider;

    /// <summary>Gets the transport options.</summary>
    protected TransportServerOptions ServerOptions { get; }

    /// <summary>Gets the internal server.</summary>
    protected TcpListener Server { get; }

    /// <summary>Gets all connected sessions.</summary>
    protected ConcurrentDictionary<string, TSession> Sessions { get; }

    /// <summary>Gets the logger.</summary>
    protected ILogger Logger { get; }

    /// <summary>Triggered when the server is stopped.</summary>
    protected CancellationToken ListeningToken =>
        _cts.Token;

    /// <summary>Gets the local endpoint.</summary>
    public EndPoint EndPoint =>
        Server.LocalEndpoint;

    /// <summary>Initializes a new instance of the <see cref="AbstractServer{TSession}" /> class.</summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="messageFactory">The message factory.</param>
    /// <param name="messageDispatcher">The message dispatcher.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The transport options.</param>
    protected AbstractServer(
        IServiceProvider provider,
        IMessageFactory messageFactory,
        IMessageDispatcher messageDispatcher,
        ILogger logger,
        IOptions<TransportServerOptions> options)
    {
        _provider = provider;
        _messageFactory = messageFactory;
        _messageDispatcher = messageDispatcher;
        _cts = new CancellationTokenSource();

        Logger = logger;
        ServerOptions = options.Value;
        Server = new TcpListener(ServerOptions.CreateEndPoint()) { ExclusiveAddressUse = true };
        Sessions = new ConcurrentDictionary<string, TSession>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Starts listening pending connections.</summary>
    public async Task StartAsync()
    {
        Server.Start();

        await OnServerStartedAsync().ConfigureAwait(false);

        while (!ListeningToken.IsCancellationRequested)
        {
            var sessionSocket = await Server.AcceptTcpClientAsync(ListeningToken).ConfigureAwait(false);

            var session = CreateSession(
                _provider.GetRequiredService<IMessageParser>(),
                _messageFactory, 
                _messageDispatcher,
                sessionSocket,
                ServerOptions,
                Logger);

            _ = OnSessionConnectedAsync(session)
                .ContinueWith(_ => session.StartListeningAsync(), ListeningToken)
                .Unwrap()
                .ContinueWith(_ => OnSessionDisconnectedAsync(session), ListeningToken)
                .Unwrap();
        }

        await OnServerStoppedAsync().ConfigureAwait(false);
    }

    /// <summary>Stops listening and removes pending connections.</summary>
    public async Task StopAsync()
    {
        Server.Stop();

        await OnServerStoppedAsync().ConfigureAwait(false);
    }

    /// <summary>Initializes a new instance of the <see cref="TSession" /> class.</summary>
    /// <param name="messageParser">The message parser.</param>
    /// <param name="messageFactory">The message factory.</param>
    /// <param name="messageDispatcher">The message dispatcher.</param>
    /// <param name="client">The internal client.</param>
    /// <param name="options">The transport options.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A new instance of <see cref="TSession" />.</returns>
    protected abstract TSession CreateSession(
        IMessageParser messageParser,
        IMessageFactory messageFactory,
        IMessageDispatcher messageDispatcher,
        TcpClient client,
        TransportServerOptions options, 
        ILogger logger);

    /// <summary>Triggered on server started.</summary>
    protected virtual ValueTask OnServerStartedAsync()
    {
        Logger.LogInformation("Server started on @{EndPoint}", EndPoint);

        return ValueTask.CompletedTask;
    }

    /// <summary>Triggered on server stopped.</summary>
    protected virtual async ValueTask OnServerStoppedAsync()
    {
        Logger.LogInformation("Server stopped on @{EndPoint}", EndPoint);

        // ReSharper disable once MethodSupportsCancellation
        await Parallel.ForEachAsync(
                Sessions.Values,
                async (session, _) =>
                {
                    await OnSessionDisconnectedAsync(session).ConfigureAwait(false);
                    await session.DisposeAsync().ConfigureAwait(false);
                })
            .ConfigureAwait(false);

        Sessions.Clear();
    }

    /// <summary>Triggered on session connected.</summary>
    /// <param name="session">The session.</param>
    protected virtual Task OnSessionConnectedAsync(TSession session)
    {
        Logger.LogInformation("Session ({Session}) connected from {EndPoint}", session, session.RemoteEndPoint);

        Sessions.TryAdd(session.SessionId, session);

        return Task.CompletedTask;
    }

    /// <summary>Triggered on session disconnected.</summary>
    /// <param name="session">The session.</param>
    protected virtual Task OnSessionDisconnectedAsync(TSession session)
    {
        Logger.LogInformation("Session ({Session}) disconnected from {EndPoint}", session, session.RemoteEndPoint);

        Sessions.TryRemove(session.SessionId, out _);

        return Task.CompletedTask;
    }
}