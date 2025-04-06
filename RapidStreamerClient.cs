using Newtonsoft.Json;
using RapidStreamer.BuildingBlocks.Application.Collections;
using RapidStreamer.BuildingBlocks.Application.Helpers;
using RapidStreamer.BuildingBlocks.Application.Objects;
using RapidStreamer.Clients.DotNet.Channels;
using RapidStreamer.Clients.DotNet.Connections.InfiniteDataStream;
using RapidStreamer.Clients.DotNet.Connections.Quic;
using RapidStreamer.Clients.DotNet.Connections.WebSocket;
using RapidStreamer.Clients.DotNet.Infrastructure.Channels;
using RapidStreamer.Clients.DotNet.Infrastructure.Connections;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using RapidStreamer.Clients.DotNet.Infrastructure.Responses;
using RapidStreamer.Clients.DotNet.Models.Enums;
using RapidStreamer.Clients.DotNet.Models.Metadata;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace RapidStreamer.Clients.DotNet
{
    public delegate void RapidStreamerConnectionStateChangedEventHandler(object sender, RapidStreamerConnectionState state, EventArgs args);

    public partial class RapidStreamerClient : DisposableObject,
        INotifyPropertyChanged
    {
        [GeneratedRegex("^(.+?),(.+?)$", RegexOptions.Compiled)]
        private static partial Regex MessageDeserializationRegex();

        private readonly ILoggerProvider _loggerProvider;
        private readonly ILogger _logger;

        private readonly BindingDictionary<string, IRapidStreamerChannel> _channels = new(true);

        public RapidStreamerProtocolType ConnectionProtocol { get; private set; }

        public IRapidStreamerConnection RapidStreamerConnection { get; }

        public string ConnectionId => RapidStreamerConnection.ConnectionId;
        public RapidStreamerConnectionState ConnectionState => RapidStreamerConnection.ConnectionState;

        private event PropertyChangedEventHandler? PropertyChanged;

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public RapidStreamerClient(RapidStreamerProtocolType connectionProtocol, AbstractRapidStreamerConfiguration configuration, ILoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
            _logger = loggerProvider.CreateLogger(GetType().GetTypeInfo().Name);
            ConnectionProtocol = connectionProtocol;

            RapidStreamerConnection = ConnectionProtocol switch
            {
                RapidStreamerProtocolType.WebSocket when configuration is RapidStreamerWebSocketConnectionConfiguration webSocketConnectionConfiguration
                    => new RapidStreamerWebSocketConnection(webSocketConnectionConfiguration, _loggerProvider),
                RapidStreamerProtocolType.InfiniteDataStream when configuration is RapidStreamerInfiniteDataStreamConnectionConfiguration infiniteDataStreamConnectionConfiguration
                    => new RapidStreamerInfiniteDataStreamConnection(infiniteDataStreamConnectionConfiguration, _loggerProvider),
                RapidStreamerProtocolType.Quic when configuration is RapidStreamerQuicConnectionConfiguration quicConnectionConfiguration
                    => new RapidStreamerQuicConnection(quicConnectionConfiguration, _loggerProvider),
                _ => throw new ArgumentOutOfRangeException()
            };

            RapidStreamerConnection.MessageReceived += RapidStreamerConnectionOnMessageReceived;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default) => await RapidStreamerConnection.ConnectAsync(cancellationToken);
        public async Task Disconnect(CancellationToken cancellationToken = default) => await RapidStreamerConnection.DisconnectAsync(cancellationToken);

        private async Task<IRapidStreamerChannel> CreateChannelAsync(string channelName, RapidStreamerChannelMetadata? channelMetadata,
            CancellationToken cancellationToken = default)
        {
            _logger.Log(LogLevel.Information, null, "Creating channel {ChannelName}", channelName);

            if (_channels.ContainsKey(channelName))
                throw new DuplicateNameException($"{channelName} already exists");

            IRapidStreamerChannel channel = ConnectionProtocol switch
            {
                RapidStreamerProtocolType.WebSocket => new RapidStreamerWebSocketChannel(channelName, RapidStreamerConnection, _loggerProvider),
                RapidStreamerProtocolType.InfiniteDataStream => new RapidStreamerInfiniteDataStreamChannel(channelName, RapidStreamerConnection, _loggerProvider),
                RapidStreamerProtocolType.Quic => new RapidStreamerQuicChannel(channelName, RapidStreamerConnection, _loggerProvider),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (!CollectionExtensions.TryAdd(_channels, channelName, channel))
                throw new InvalidOperationException();

            if (channelMetadata is not null)
                channel.SetChannelMetadata(channelMetadata);
            else
                await channel.RequestChannelMetadataAsync(true, cancellationToken);

            _logger.Log(LogLevel.Information, null, "Channel {ChannelName} created", channelName);

            return channel;
        }

        public Task<IRapidStreamerChannel> CreateChannelAsync(RapidStreamerChannelMetadata channelMetadata, CancellationToken cancellationToken = default)
            => CreateChannelAsync(channelMetadata.ChannelName, channelMetadata, cancellationToken);

        public Task<IRapidStreamerChannel> CreateChannelAsync(string channelName, CancellationToken cancellationToken = default)
            => CreateChannelAsync(channelName, null, cancellationToken);

        public IRapidStreamerChannel GetChannel(string channelName) => _channels[channelName];

        private async Task RapidStreamerConnectionOnMessageReceived(object sender, string message, CancellationToken cancellationToken = default)
        {
            if (message.TrimStart().StartsWith('{'))
            {
                _logger.Log(LogLevel.Information, null, "Json message has received {Message} created, {Sender}", message, sender.ToNJson());

                var clientBaseResponse = message.FromNJson<RapidStreamerResponseBase>() ?? throw new JsonException();
                var channel = _channels[clientBaseResponse.Route.Channel];
                channel.HandleReceivedResponse(clientBaseResponse);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                var match = MessageDeserializationRegex().Match(message);
                if (match.Success)
                {
                    var channel = _channels[match.Groups[1].Value];
                    var receivedMessage = match.Groups[2].Value;

                    await channel.HandleReceivedMessageAsync(receivedMessage, cancellationToken);

                    _logger.Log(LogLevel.Information, null, $"Handled => Time {nameof(Stopwatch.Elapsed)} is {stopwatch.Elapsed}");

                    stopwatch.Stop();
                }
            }
        }

        protected override void DisposeManagedResources() => RapidStreamerConnection.Dispose();
    }
}