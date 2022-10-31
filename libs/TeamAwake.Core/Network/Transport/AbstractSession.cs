using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TeamAwake.Core.Network.Dispatchers;
using TeamAwake.Core.Network.Factories;
using TeamAwake.Core.Network.Metadatas;
using TeamAwake.Core.Network.Options;
using TeamAwake.Core.Network.Parsers;

#pragma warning disable CA2254
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace TeamAwake.Core.Network.Transport;

/// <summary>Describes a way to manage a server connection.</summary>
public abstract class AbstractSession : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly IMessageDispatcher _messageDispatcher;
    private readonly IMessageFactory _messageFactory;
    private readonly IMessageParser _messageParser;

    private bool _disposed;
    private Exception? _receiveError;
    private Exception? _sendError;
    private string? _sessionId;

    /// <summary>Gets the internal client.</summary>
    protected TcpClient Client { get; }

    /// <summary>Gets the pipe reader.</summary>
    protected PipeReader PipeReader { get; }

    /// <summary>Gets the pipe writer.</summary>
    protected PipeWriter PipeWriter { get; }

    /// <summary>Gets the logger.</summary>
    protected ILogger Logger { get; }

    /// <summary>Gets the transport options.</summary>
    protected TransportServerOptions ServerOptions { get; }

    /// <summary>Gets the remote endpoint.</summary>
    public IPEndPoint RemoteEndPoint =>
        (IPEndPoint)Client.Client.RemoteEndPoint!;

    /// <summary>Triggered when the session is closed.</summary>
    public CancellationToken SessionClosed =>
        _cts.Token;

    /// <summary>Gets the unique identifier of the session.</summary>
    public string SessionId =>
        _sessionId ??= Guid.NewGuid().ToString("N");

    /// <summary>Initializes a new instance of the <see cref="AbstractSession" /> class.</summary>
    /// <param name="messageParser">The message parser.</param>
    /// <param name="messageFactory">The message factory.</param>
    /// <param name="messageDispatcher">The message dispatcher.</param>
    /// <param name="serverOptions">The transport options.</param>
    /// <param name="client">The internal client.</param>
    /// <param name="logger">The logger.</param>
    protected AbstractSession(
        IMessageParser messageParser,
        IMessageFactory messageFactory,
        IMessageDispatcher messageDispatcher,
        TransportServerOptions serverOptions,
        TcpClient client,
        ILogger logger)
    {
        _messageParser = messageParser;
        _messageFactory = messageFactory;
        _messageDispatcher = messageDispatcher;
        _cts = new CancellationTokenSource();

        Client = client;
        ServerOptions = serverOptions;
        PipeReader = PipeReader.Create(Client.GetStream());
        PipeWriter = PipeWriter.Create(Client.GetStream());
        Logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (!_cts.IsCancellationRequested)
            _cts.Cancel();

        await PipeReader.CompleteAsync(_receiveError).ConfigureAwait(false);
        await PipeWriter.CompleteAsync(_sendError).ConfigureAwait(false);

        _cts.Dispose();
        Client.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>Starts listening pending data from the <see cref="Client" />.</summary>
    internal async Task StartListeningAsync()
    {
        try
        {
            while (!SessionClosed.IsCancellationRequested)
            {
                var readResult = await PipeReader.ReadAsync(SessionClosed).ConfigureAwait(false);

                if (readResult.IsCanceled)
                    break;

                var buffer = readResult.Buffer;

                try
                {
                    foreach (var message in _messageParser.DecodeMessages(buffer, this))
                    {
                        var dispatchResult = await _messageDispatcher.DispatchMessageAsync(message, this).ConfigureAwait(false);

                        if (!Logger.IsEnabled(LogLevel.Debug) || !ServerOptions.LogMessages)
                            continue;

                        var messageName = _messageFactory.GetMessageName(message.Identifier);

                        Logger.LogDebug(
                            dispatchResult switch
                            {
                                DispatchResults.Success => "Session {@Session} received message {MessageName}",
                                DispatchResults.Unhandled => "Session {@Session} received message {MessageName} but no handler was found for it",
                                DispatchResults.Failure or _ => "Session {@Session} received message {MessageName} but an error occurred while dispatching it"
                            },
                            this,
                            messageName);
                    }

                    if (readResult.IsCompleted)
                    {
                        if (!buffer.IsEmpty)
                            throw new InvalidDataException("Incomplete message received");

                        break;
                    }
                }
                finally
                {
                    PipeReader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
        }
        catch (Exception e)
        {
            _receiveError = e;

            Logger.LogError(e, "An error occurred while receiving data from the Session ({Id})", SessionId);
        }
    }

    /// <summary>Stops all pending actions.</summary>
    public void Disconnect()
    {
        if (_cts.IsCancellationRequested)
            return;

        PipeReader.CancelPendingRead();
        PipeWriter.CancelPendingFlush();

        _cts.Cancel();
    }

    /// <summary>Sends asynchronously a network message.</summary>
    /// <param name="message">The message to send.</param>
    /// <exception cref="OperationCanceledException">Whether the operation is canceled.</exception>
    public async Task SendAsync(WakfuMessage message)
    {
        if (SessionClosed.IsCancellationRequested)
            return;

        var buffer = _messageParser.EncodeMessage(message);

        try
        {
            var flushResult = buffer.IsEmpty
                ? await PipeWriter.FlushAsync(SessionClosed).ConfigureAwait(false)
                : await PipeWriter.WriteAsync(buffer, SessionClosed).ConfigureAwait(false);

            if (flushResult.IsCanceled)
                Disconnect();
        }
        catch (Exception e)
        {
            _sendError = e;

            Logger.LogError(e, "An error occurred while sending data from the Session ({@Session})", this);
        }
    }
    
    /// <inheritdoc />
    public override string ToString() =>
        SessionId;
}