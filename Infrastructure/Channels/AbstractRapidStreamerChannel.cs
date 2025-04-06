using Ardalis.GuardClauses;
using Newtonsoft.Json.Linq;
using RapidStreamer.BuildingBlocks.Application;
using RapidStreamer.BuildingBlocks.Application.Ciphering;
using RapidStreamer.BuildingBlocks.Application.Collections;
using RapidStreamer.BuildingBlocks.Application.Helpers;
using RapidStreamer.Clients.DotNet.Infrastructure.Connections;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using RapidStreamer.Clients.DotNet.Infrastructure.Requests;
using RapidStreamer.Clients.DotNet.Infrastructure.Responses;
using RapidStreamer.Clients.DotNet.Models;
using RapidStreamer.Clients.DotNet.Models.Enums;
using RapidStreamer.Clients.DotNet.Models.Metadata;
using RapidStreamer.Clients.DotNet.Models.ReceivedMessage;
using RapidStreamer.Clients.DotNet.Models.Requests;
using RapidStreamer.Clients.DotNet.Models.Subscriptions;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace RapidStreamer.Clients.DotNet.Infrastructure.Channels
{
    public delegate void RapidStreamerChannelStatusChangedEventHandler(object sender, RapidStreamerChannelState state, EventArgs args);

    public delegate void RapidStreamerMetadataUpdatedEventHandler(object sender, RapidStreamerChannelMetadata channelMetadata, EventArgs args);

    public delegate void RapidStreamerChannelReceivedMessageEventHandler(object sender, ChannelReceivedMessage channelReceivedMessage, EventArgs args);

    public abstract partial class AbstractRapidStreamerChannel : INotifyPropertyChanged,
        IRapidStreamerChannel
    {
        private readonly BindingDictionary<string, RapidStreamerRequestBase> _requests = new(true);
        private readonly BindingDictionary<string, RapidStreamerSubscription> _subscriptions = new(true);

        private RapidStreamerChannelState _channelStatus = RapidStreamerChannelState.Ready;
        private RapidStreamerChannelMetadata? _channelMetadata;

        private string? _token;
        private string? _username;
        private string? _password;
        private CipheringMetadata? _authEncryptorCipheringMetadata;
        private Func<string, string> _authEncryptor = param => param;

        private CipheringMetadata? _messageDecryptorCipheringMetadata;
        private Func<string, string> _messageDecryptor = message => message;

        [GeneratedRegex("^(.+?),(.+?),(.+?),(.+?),(.*)$", RegexOptions.Compiled)]
        private static partial Regex MessageDeserializationRegex();

        protected string Name { get; }
        protected ILoggerProvider LoggerProvider { get; }
        protected ILogger Logger { get; }
        protected IRapidStreamerConnection RapidStreamerConnection { get; }

        public RapidStreamerChannelMetadata? ChannelMetadata
        {
            get => _channelMetadata;
            protected set
            {
                if (SetField(ref _channelMetadata, value) && _channelMetadata is not null)
                {
                    MetadataUpdated?.Invoke(this, _channelMetadata, EventArgs.Empty);
                    BuildAuthEncryptor();
                    BuildMessageDecryptor();
                }
            }
        }

        public RapidStreamerChannelState ChannelStatus
        {
            get => _channelStatus;
            protected set
            {
                if (SetField(ref _channelStatus, value))
                    ChannelStatusChanged?.Invoke(this, _channelStatus, EventArgs.Empty);
            }
        }

        public RapidStreamerPingPongState PingPongState { get; private set; } = RapidStreamerPingPongState.NotPinged;

        public event RapidStreamerChannelStatusChangedEventHandler? ChannelStatusChanged;
        public event RapidStreamerMetadataUpdatedEventHandler? MetadataUpdated;
        public event RapidStreamerChannelReceivedMessageEventHandler? ReceivedMessage;
        public event PropertyChangedEventHandler? PropertyChanged;

        internal AbstractRapidStreamerChannel(string name, IRapidStreamerConnection rapidStreamerConnection, ILoggerProvider loggerProvider)
        {
            Name = name;
            RapidStreamerConnection = rapidStreamerConnection;
            LoggerProvider = loggerProvider;
            Logger = loggerProvider.CreateLogger(GetType().GetTypeInfo().Name);

            Logger.Log(LogLevel.Information, null, "Channel {Channel} has configured properly", this);
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

        private void BuildAuthEncryptor()
        {
            ArgumentNullException.ThrowIfNull(ChannelMetadata, nameof(ChannelMetadata));
            _authEncryptor = ChannelMetadata.Authentication switch
            {
                { IsEnabled: true, AuthenticationType: RapidStreamerChannelAuthenticationType.Basic } when _authEncryptorCipheringMetadata is not null =>
                    param => RsaEncryptionService.Encrypt(param, _authEncryptorCipheringMetadata.Key, _authEncryptorCipheringMetadata.KeySize),
                _ => param => param
            };
        }

        private void BuildMessageDecryptor()
        {
            ArgumentNullException.ThrowIfNull(ChannelMetadata, nameof(ChannelMetadata));
            _messageDecryptor = ChannelMetadata.MessageEncryption.IsEnabled switch
            {
                true when _messageDecryptorCipheringMetadata is not null =>
                    message => RsaEncryptionService.Decrypt(message, _messageDecryptorCipheringMetadata.Key, _messageDecryptorCipheringMetadata.KeySize),
                _ => message => message
            };
        }

        private async Task RequestAsync<TRequest>(TRequest request, bool awaitable = false, CancellationToken cancellationToken = default)
            where TRequest : RapidStreamerRequestBase
        {
            Logger.Log(LogLevel.Information, null, "Requesting {Request}({Awaitable})", request, awaitable);

            ManualResetEvent? requestManualResetEvent = null;
            if (awaitable)
                requestManualResetEvent = request.SetAwaitable();

            if (!_requests.ContainsKey(request.RequestId) && !CollectionExtensions.TryAdd(_requests, request.RequestId, request))
                throw new InvalidOperationException();

            if (request is not RapidStreamerMetadataRequest and RapidStreamerPingRequest)
            {
                ArgumentNullException.ThrowIfNull(ChannelMetadata, nameof(ChannelMetadata));

                if (ChannelMetadata.Authentication.IsEnabled)
                {
                    switch (ChannelMetadata.Authentication.AuthenticationType)
                    {
                        case RapidStreamerChannelAuthenticationType.OAuth2:
                            request.Token = Guard.Against.NullOrWhiteSpace(_token);
                            break;
                        case RapidStreamerChannelAuthenticationType.Basic:
                            request.Username = _authEncryptor.Invoke(Guard.Against.NullOrWhiteSpace(_username));
                            request.Password = _authEncryptor.Invoke(Guard.Against.NullOrWhiteSpace(_password));
                            break;
                    }
                }
            }


            await RapidStreamerConnection.SendAsync(request.ToNJson(), cancellationToken);
            request.SetRequestTimeout(TimeSpan.FromSeconds(40), () =>
            {
                _requests.Remove(request.RequestId, out _);
                request.Dispose();
            });

            requestManualResetEvent?.WaitOne();

            Logger.Log(LogLevel.Information, null, "{Request}({Awaitable}) has requested", request, awaitable);
        }

        public void SetToken(string token) => _token = token;

        public void SetAuthentication(string username, string password, CipheringMetadata cipheringMetadata)
        {
            _username = username;
            _password = password;
            _authEncryptorCipheringMetadata = cipheringMetadata;
        }

        public void SetMessageCipheringMetadata(CipheringMetadata cipheringMetadata)
        {
            _messageDecryptorCipheringMetadata = cipheringMetadata;
            BuildMessageDecryptor();
        }

        public void HandleReceivedResponse(RapidStreamerResponseBase response)
        {
            var content = response.ResponseContent.FromNJson<JObject>()!;
            if (_requests.Remove(response.RequestId, out var request))
            {
                try
                {
                    if (response.ResponseCode != 200)
                    {
                        if (content.TryGetValue(nameof(Exception), StringComparison.InvariantCultureIgnoreCase, out var exception))
                        {
                            var exceptionInfo = exception.ToString().FromJson<ExceptionInfo>();
                            if (exceptionInfo is not null)
                                throw new AggregateException(exceptionInfo.Message);
                        }

                        throw new Exception(content.ToString());
                    }

                    switch (request)
                    {
                        case RapidStreamerPingRequest:
                            Console.WriteLine(content);
                            PingPongState = RapidStreamerPingPongState.Ponged;
                            break;
                        case RapidStreamerMetadataRequest:
                            try
                            {
                                if (content.TryGetValue("Metadata", StringComparison.InvariantCultureIgnoreCase, out var jToken))
                                {
                                    ChannelMetadata = Guard.Against.Null(jToken.ToObject<RapidStreamerChannelMetadata>(),
                                        nameof(ChannelMetadata),
                                        "An error has occured while Channel metadata request process");
                                    ChannelStatus = RapidStreamerChannelState.HasMetadata;
                                }
                                else
                                    ChannelStatus = RapidStreamerChannelState.HasError;
                            }
                            catch (Exception e)
                            {
                                Logger.Log(LogLevel.Error, e, null);
                                ChannelStatus = RapidStreamerChannelState.HasError;
                                throw;
                            }

                            break;
                        case RapidStreamerSubscriptionRequest subscriptionRequest:
                            ArgumentNullException.ThrowIfNull(subscriptionRequest.RapidStreamerSubscription);
                            AddSubscription(subscriptionRequest);
                            break;
                        case RapidStreamerUnsubscribeRequest unsubscribeRequest:
                            ArgumentNullException.ThrowIfNull(unsubscribeRequest.RapidStreamerSubscription);
                            if (_subscriptions.Remove(unsubscribeRequest.RequestId, out _))
                                unsubscribeRequest.SetUnsubscribed();
                            break;
                    }
                }
                finally
                {
                    request.Dispose();
                }
            }
        }

        public async Task HandleReceivedMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ChannelMetadata, nameof(ChannelMetadata));

            message = _messageDecryptor(message);

            var match = MessageDeserializationRegex().Match(message);
            if (match.Success)
            {
                ChannelReceivedMessageHeader channelReceivedMessageHeader = new(ChannelMetadata.ChannelName,
                    RestoreSplitters(match.Groups[1].Value),
                    match.Groups[2].Value.Equals("1"),
                    match.Groups[3].Value);

                var channelReceivedMessageKeys = match.Groups[4].Value.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(RestoreSplitters).ToArray();

                var channelReceivedMessageValues = match.Groups[5].Value.Split(';').Select(item =>
                {
                    var parts = item.Split('=');
                    return new KeyValuePair<int, string>(int.Parse(parts[0]), RestoreSplitters(parts[1]));
                }).ToDictionary();

                ChannelReceivedMessage channelReceivedMessage = new(channelReceivedMessageHeader, channelReceivedMessageKeys, channelReceivedMessageValues);

                ReceivedMessage?.Invoke(this, channelReceivedMessage, EventArgs.Empty);

                var subscription = _subscriptions[channelReceivedMessage.Header.RequestId];
                await subscription.HandleReceivedMessageAsync(channelReceivedMessage, cancellationToken);
            }

            return;

            string RestoreComma(string str) => str.Contains("<C>") ? str.Replace("<C>", ",") : str;

            string RestorePipe(string str) => str.Contains("<PI>") ? str.Replace("<PI>", "|") : str;

            string RestoreSemicolon(string str) => str.Contains("<SC>") ? str.Replace("<SC>", ";") : str;

            string RestoreSplitters(string str)
            {
                str = RestoreComma(str);
                str = RestorePipe(str);
                str = RestoreSemicolon(str);

                return str;
            }
        }

        public async Task RequestPingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                PingPongState = RapidStreamerPingPongState.Pinging;

                await RequestAsync(new RapidStreamerPingRequest(RequestIdHelper.Generate(), Name), true, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                ChannelStatus = RapidStreamerChannelState.HasError;
                Logger.Log(LogLevel.Error, exception, null);
                throw;
            }
        }

        public virtual void SetChannelMetadata(RapidStreamerChannelMetadata channelMetadata)
        {
            try
            {
                ChannelStatus = RapidStreamerChannelState.RequestingMetadata;

                ChannelMetadata = channelMetadata;

                ChannelStatus = RapidStreamerChannelState.HasMetadata;
            }
            catch (Exception exception)
            {
                ChannelStatus = RapidStreamerChannelState.HasError;
                Logger.Log(LogLevel.Error, exception, null);
                throw;
            }
        }

        public virtual async Task RequestChannelMetadataAsync(bool awaitable, CancellationToken cancellationToken = default)
        {
            try
            {
                ChannelStatus = RapidStreamerChannelState.RequestingMetadata;

                await RequestAsync(new RapidStreamerMetadataRequest(RequestIdHelper.Generate(), Name), awaitable, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                ChannelStatus = RapidStreamerChannelState.HasError;
                Logger.Log(LogLevel.Error, exception, null);
                throw;
            }
        }

        public Task RequestChannelMetadataAsync(CancellationToken cancellationToken = default) => RequestChannelMetadataAsync(false, cancellationToken: cancellationToken);

        public virtual Task RequestSubscriptionAsync(RapidStreamerSubscriptionRequest request, CancellationToken cancellationToken = default)
            => RequestAsync(request, false, cancellationToken);

        public virtual Task RequestUnsubscribeAsync(RapidStreamerUnsubscribeRequest request, CancellationToken cancellationToken = default)
            => RequestAsync(request, false, cancellationToken);

        public IRapidStreamerSubscriptionOperational CreateSubscription(IReadOnlyDictionary<string, string> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            RapidStreamerSubscriptionMode subscriptionMode = RapidStreamerSubscriptionMode.Full)
            => new RapidStreamerSubscription(this, subscribingKeys, subscribingFields, subscriptionMode, LoggerProvider);

        public IRapidStreamerSubscriptionOperational CreateSubscription(IReadOnlyCollection<IReadOnlyDictionary<string, string>> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            RapidStreamerSubscriptionMode subscriptionMode = RapidStreamerSubscriptionMode.Full)
            => new RapidStreamerSubscriptions(this, subscribingKeys, subscribingFields, subscriptionMode, LoggerProvider);

        protected void AddSubscription(RapidStreamerSubscriptionRequest subscriptionRequest)
        {
            if (CollectionExtensions.TryAdd(_subscriptions, subscriptionRequest.RequestId, subscriptionRequest.RapidStreamerSubscription))
                subscriptionRequest.SetSubscribed();
        }
    }
}