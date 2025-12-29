using Newtonsoft.Json;
using ThunderPropagator.BuildingBlocks.Application.Helpers;
using ThunderPropagator.BuildingBlocks.Application.Objects;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Connections;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ThunderPropagator.Clients.DotNet.Infrastructure.Connections
{
    public delegate Task ThunderPropagatorMessageReceivedEventHandler(object sender, string message, CancellationToken cancellationToken = default);

    internal abstract class AbstractThunderPropagatorConnection<TConnectionConfiguration> : DisposableObject,
        IThunderPropagatorConnection,
        INotifyPropertyChanged
        where TConnectionConfiguration : AbstractThunderPropagatorConfiguration
    {
        private bool _gotConnectionInfo;
        private ThunderPropagatorConnectionState _connectionState = ThunderPropagatorConnectionState.Ready;
        private CancellationTokenSource? _receivingCancellationTokenSource;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        protected ILogger Logger { get; }
        protected TConnectionConfiguration ConnectionConfiguration { get; }

        public string ConnectionId => ConnectionInfo.ConnectionId;

        public ThunderPropagatorConnectionState ConnectionState
        {
            get => _connectionState;
            private set
            {
                if (SetField(ref _connectionState, value))
                    ConnectionStateChanged?.Invoke(this, _connectionState, EventArgs.Empty);
            }
        }

        public ThunderPropagatorConnectionResponse ConnectionInfo { get; private set; } = null!;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event ThunderPropagatorConnectionStateChangedEventHandler? ConnectionStateChanged;
        public event ThunderPropagatorMessageReceivedEventHandler? MessageReceived;

        protected AbstractThunderPropagatorConnection(TConnectionConfiguration connectionConfiguration, ILoggerProvider loggerProvider)
        {
            ConnectionConfiguration = connectionConfiguration;
            Logger = loggerProvider.CreateLogger(GetType().GetTypeInfo().Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual async Task OnMessageReceived(string message, CancellationToken cancellationToken = default)
        {
            if (message.StartsWith("PROBE"))
                return;

            if (!_gotConnectionInfo)
            {
                ConnectionInfo = message.FromNJson<ThunderPropagatorConnectionResponse>() ?? throw new JsonException();
                _gotConnectionInfo = true;
            }
            else if (MessageReceived is not null)
                await MessageReceived.Invoke(this, message, cancellationToken);
        }

        async Task IThunderPropagatorConnection.ConnectAsync(CancellationToken cancellationToken)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);

            try
            {
                if (ConnectionState is ThunderPropagatorConnectionState.Closed or ThunderPropagatorConnectionState.Ready or ThunderPropagatorConnectionState.HasError)
                {
                    ConnectionState = ThunderPropagatorConnectionState.Connecting;
                    ManualResetEvent manualResetEvent = new(false);

                    using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(20));
                    await InternalConnectAsync(cancellationTokenSource.Token);

                    _ = Task
                        .Run(async () =>
                        {
                            while (ConnectionState is ThunderPropagatorConnectionState.Connecting or ThunderPropagatorConnectionState.Open or ThunderPropagatorConnectionState.HasError)
                            {
                                _receivingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                                _receivingCancellationTokenSource.CancelAfter(TimeSpan.FromHours(24));

                                try
                                {
                                    var message = await ReceiveAsync(_receivingCancellationTokenSource.Token);

                                    if (ConnectionState is ThunderPropagatorConnectionState.Connecting)
                                        ConnectionState = ThunderPropagatorConnectionState.Open;

                                    StringBuilder stringBuilder = new(message.Trim());
                                    stringBuilder.Replace("\0", string.Empty);

                                    await OnMessageReceived(stringBuilder.ToString().Trim(), _receivingCancellationTokenSource.Token);

                                    manualResetEvent.Set();
                                }
                                catch (Exception exception)
                                {
                                    ConnectionState = ThunderPropagatorConnectionState.HasError;
                                    Logger.Log(LogLevel.Error, exception, null);
                                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken: CancellationToken.None);
                                }
                                finally
                                {
                                    _receivingCancellationTokenSource.Dispose();
                                }
                            }
                        }, CancellationToken.None)
                        .ContinueWith(async task =>
                        {
                            try
                            {
                                await task;
                            }
                            finally
                            {
                                task.Dispose();
                            }
                        }, CancellationToken.None);

                    manualResetEvent.WaitOne();
                }
            }
            catch (Exception exception)
            {
                ConnectionState = ThunderPropagatorConnectionState.HasError;
                Logger.Log(LogLevel.Error, exception, null);
                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        protected abstract Task InternalConnectAsync(CancellationToken cancellationToken = default);

        async Task IThunderPropagatorConnection.DisconnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                ConnectionState = ThunderPropagatorConnectionState.Closing;

                if (_receivingCancellationTokenSource is not null)
                    await _receivingCancellationTokenSource.CancelAsync();

                using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(20));
                await InternalDisconnectAsync(cancellationTokenSource.Token);

                ConnectionState = ThunderPropagatorConnectionState.Closed;
            }
            catch (Exception exception)
            {
                ConnectionState = ThunderPropagatorConnectionState.HasError;
                Logger.Log(LogLevel.Error, exception, null);
            }
        }

        protected abstract Task InternalDisconnectAsync(CancellationToken cancellationToken = default);

        protected abstract ValueTask<string> ReceiveAsync(CancellationToken cancellationToken = default);

        async Task IThunderPropagatorConnection.SendAsync(string message, CancellationToken cancellationToken)
        {
            if (ConnectionState is ThunderPropagatorConnectionState.Open)
            {
                using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(20));
                await InternalSendAsync(message, cancellationTokenSource.Token);
            }
        }

        protected abstract Task InternalSendAsync(string message, CancellationToken cancellationToken);
    }
}