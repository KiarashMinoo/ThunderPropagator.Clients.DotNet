using Ardalis.GuardClauses;
using ThunderPropagator.BuildingBlocks.Application.Helpers;
using ThunderPropagator.Clients.DotNet.Connections.InfiniteDataStream;
using ThunderPropagator.Clients.DotNet.Infrastructure.Channels;
using ThunderPropagator.Clients.DotNet.Infrastructure.Connections;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using ThunderPropagator.Clients.DotNet.Models.Metadata;
using ThunderPropagator.Clients.DotNet.Models.Requests;
using System.Text;

namespace ThunderPropagator.Clients.DotNet.Channels
{
    internal
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorInfiniteDataStreamChannel : AbstractThunderPropagatorChannel
    {
        private ThunderPropagatorInfiniteDataStreamConnection ThunderPropagatorInfiniteDataStreamConnection => (ThunderPropagatorInfiniteDataStreamConnection)ThunderPropagatorConnection;

        public ThunderPropagatorInfiniteDataStreamChannel(string name, IThunderPropagatorConnection thunderPropagatorConnection, ILoggerProvider loggerProvider)
            : base(name, thunderPropagatorConnection, loggerProvider)
        {
        }

        public override async Task RequestChannelMetadataAsync(bool awaitable, CancellationToken cancellationToken = default)
        {
            try
            {
                ChannelStatus = ThunderPropagatorChannelState.RequestingMetadata;

                try
                {
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"/channel/{Name}/metadata");
                    var httpResponseMessage = await ThunderPropagatorInfiniteDataStreamConnection.SendAsync(httpRequestMessage, cancellationToken: cancellationToken);
                    httpResponseMessage.EnsureSuccessStatusCode();
                    var json = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                    ChannelMetadata = Guard.Against.Null(json.FromNJson<ThunderPropagatorChannelMetadata>(), nameof(ChannelMetadata),
                        "An error has occured while Channel metadata request process");
                    ChannelStatus = ThunderPropagatorChannelState.HasMetadata;
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e, null);
                    ChannelStatus = ThunderPropagatorChannelState.HasError;
                    throw;
                }
            }
            catch (Exception exception)
            {
                ChannelStatus = ThunderPropagatorChannelState.HasError;
                Logger.Log(LogLevel.Error, exception, null);
                throw;
            }
        }

        public override Task RequestSubscriptionAsync(ThunderPropagatorSubscriptionRequest request, CancellationToken _ = default)
        {
            Task
                .Run(async () =>
                {
                    try
                    {
                        var count = 0;
                        using var httpRequestMessage =
                            new HttpRequestMessage(HttpMethod.Post, $"/{Guard.Against.Null(ChannelMetadata, nameof(ChannelMetadata)).ChannelName}/subscribe");
                        httpRequestMessage.Content = new StringContent(request.ToNJson(), Encoding.UTF8, "application/json");

                        using var httpResponseMessage =
                            await ThunderPropagatorInfiniteDataStreamConnection.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken: CancellationToken.None);
                        var chunkedEncodingReadStream = await httpResponseMessage.Content.ReadAsStreamAsync(CancellationToken.None);
                        using StreamReader streamReader = new(chunkedEncodingReadStream);
                        while (ThunderPropagatorConnection.ConnectionState is ThunderPropagatorConnectionState.Connecting or ThunderPropagatorConnectionState.Open
                                   or ThunderPropagatorConnectionState.HasError)
                        {
                            var line = await streamReader.ReadLineAsync(CancellationToken.None);
                            if (line is null)
                                break;
                            
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                count++;

                                if (count == 2)
                                    AddSubscription(request);

                                await ThunderPropagatorInfiniteDataStreamConnection.AddMessageAsync(line, cancellationToken: CancellationToken.None);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Log(LogLevel.Error, exception, null);
                        throw;
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

            return Task.CompletedTask;
        }

        public override Task RequestUnsubscribeAsync(ThunderPropagatorUnsubscribeRequest request, CancellationToken cancellationToken = default)
            => ThunderPropagatorConnection.DisconnectAsync(cancellationToken);
    }
}