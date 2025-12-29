using Newtonsoft.Json;
using ThunderPropagator.BuildingBlocks.Application.Collections;
using ThunderPropagator.BuildingBlocks.Application.Helpers;
using ThunderPropagator.BuildingBlocks.Application.Objects;
using ThunderPropagator.Clients.DotNet.Channels;
using ThunderPropagator.Clients.DotNet.Connections.InfiniteDataStream;
using ThunderPropagator.Clients.DotNet.Connections.Quic;
using ThunderPropagator.Clients.DotNet.Connections.WebSocket;
using ThunderPropagator.Clients.DotNet.Infrastructure.Channels;
using ThunderPropagator.Clients.DotNet.Infrastructure.Connections;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Infrastructure.Responses;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using ThunderPropagator.Clients.DotNet.Models.Metadata;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ThunderPropagator.Clients.DotNet
{
    public delegate void ThunderPropagatorConnectionStateChangedEventHandler(object sender, ThunderPropagatorConnectionState state, EventArgs args);

    public partial class ThunderPropagatorClient : DisposableObject,
        INotifyPropertyChanged
    {
        [GeneratedRegex("^(.+?),(.+?)$", RegexOptions.Compiled)]
        private static partial Regex MessageDeserializationRegex();

        private readonly ILoggerProvider _loggerProvider;
        private readonly ILogger _logger;

        private readonly BindingDictionary<string, IThunderPropagatorChannel> _channels = new(true);

        public ThunderPropagatorProtocolType ConnectionProtocol { get; private set; }

        public IThunderPropagatorConnection ThunderPropagatorConnection { get; }

        public string ConnectionId => ThunderPropagatorConnection.ConnectionId;
        public ThunderPropagatorConnectionState ConnectionState => ThunderPropagatorConnection.ConnectionState;

        private event PropertyChangedEventHandler? PropertyChanged;

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public ThunderPropagatorClient(ThunderPropagatorProtocolType connectionProtocol, AbstractThunderPropagatorConfiguration configuration, ILoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
            _logger = loggerProvider.CreateLogger(GetType().GetTypeInfo().Name);
            ConnectionProtocol = connectionProtocol;

            ThunderPropagatorConnection = ConnectionProtocol switch
            {
                ThunderPropagatorProtocolType.WebSocket when configuration is ThunderPropagatorWebSocketConnectionConfiguration webSocketConnectionConfiguration
                    => new ThunderPropagatorWebSocketConnection(webSocketConnectionConfiguration, _loggerProvider),
                ThunderPropagatorProtocolType.InfiniteDataStream when configuration is ThunderPropagatorInfiniteDataStreamConnectionConfiguration infiniteDataStreamConnectionConfiguration
                    => new ThunderPropagatorInfiniteDataStreamConnection(infiniteDataStreamConnectionConfiguration, _loggerProvider),
                ThunderPropagatorProtocolType.Quic when configuration is ThunderPropagatorQuicConnectionConfiguration quicConnectionConfiguration
                    => new ThunderPropagatorQuicConnection(quicConnectionConfiguration, _loggerProvider),
                _ => throw new ArgumentOutOfRangeException()
            };

            ThunderPropagatorConnection.MessageReceived += ThunderPropagatorConnectionOnMessageReceived;
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

        public async Task ConnectAsync(CancellationToken cancellationToken = default) => await ThunderPropagatorConnection.ConnectAsync(cancellationToken);
        public async Task Disconnect(CancellationToken cancellationToken = default) => await ThunderPropagatorConnection.DisconnectAsync(cancellationToken);

        private async Task<IThunderPropagatorChannel> CreateChannelAsync(string channelName, ThunderPropagatorChannelMetadata? channelMetadata,
            CancellationToken cancellationToken = default)
        {
            _logger.Log(LogLevel.Information, null, "Creating channel {ChannelName}", channelName);

            if (_channels.ContainsKey(channelName))
                throw new DuplicateNameException($"{channelName} already exists");

            IThunderPropagatorChannel channel = ConnectionProtocol switch
            {
                ThunderPropagatorProtocolType.WebSocket => new ThunderPropagatorWebSocketChannel(channelName, ThunderPropagatorConnection, _loggerProvider),
                ThunderPropagatorProtocolType.InfiniteDataStream => new ThunderPropagatorInfiniteDataStreamChannel(channelName, ThunderPropagatorConnection, _loggerProvider),
                ThunderPropagatorProtocolType.Quic => new ThunderPropagatorQuicChannel(channelName, ThunderPropagatorConnection, _loggerProvider),
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

        public Task<IThunderPropagatorChannel> CreateChannelAsync(ThunderPropagatorChannelMetadata channelMetadata, CancellationToken cancellationToken = default)
            => CreateChannelAsync(channelMetadata.ChannelName, channelMetadata, cancellationToken);

        public Task<IThunderPropagatorChannel> CreateChannelAsync(string channelName, CancellationToken cancellationToken = default)
            => CreateChannelAsync(channelName, null, cancellationToken);

        public IThunderPropagatorChannel GetChannel(string channelName) => _channels[channelName];

        private async Task ThunderPropagatorConnectionOnMessageReceived(object sender, string message, CancellationToken cancellationToken = default)
        {
            if (message.TrimStart().StartsWith('{'))
            {
                _logger.Log(LogLevel.Information, null, "Json message has received {Message} created, {Sender}", message, sender.ToNJson());

                var clientBaseResponse = message.FromNJson<ThunderPropagatorResponseBase>() ?? throw new JsonException();
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

        protected override void DisposeManagedResources() => ThunderPropagatorConnection.Dispose();
    }
}