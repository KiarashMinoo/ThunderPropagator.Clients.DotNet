using Ardalis.GuardClauses;
using RapidStreamer.BuildingBlocks.Application.Helpers;
using RapidStreamer.Clients.DotNet.Connections.InfiniteDataStream;
using RapidStreamer.Clients.DotNet.Infrastructure.Channels;
using RapidStreamer.Clients.DotNet.Infrastructure.Connections;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using RapidStreamer.Clients.DotNet.Models.Enums;
using RapidStreamer.Clients.DotNet.Models.Metadata;
using RapidStreamer.Clients.DotNet.Models.Requests;
using System.Text;

namespace RapidStreamer.Clients.DotNet.Channels
{
    internal
#if !DEBUG
        sealed
#endif
        class RapidStreamerInfiniteDataStreamChannel : AbstractRapidStreamerChannel
    {
        private RapidStreamerInfiniteDataStreamConnection RapidStreamerInfiniteDataStreamConnection => (RapidStreamerInfiniteDataStreamConnection)RapidStreamerConnection;

        public RapidStreamerInfiniteDataStreamChannel(string name, IRapidStreamerConnection rapidStreamerConnection, ILoggerProvider loggerProvider)
            : base(name, rapidStreamerConnection, loggerProvider)
        {
        }

        public override async Task RequestChannelMetadataAsync(bool awaitable, CancellationToken cancellationToken = default)
        {
            try
            {
                ChannelStatus = RapidStreamerChannelState.RequestingMetadata;

                try
                {
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"/channel/{Name}/metadata");
                    var httpResponseMessage = await RapidStreamerInfiniteDataStreamConnection.SendAsync(httpRequestMessage, cancellationToken: cancellationToken);
                    httpResponseMessage.EnsureSuccessStatusCode();
                    var json = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                    ChannelMetadata = Guard.Against.Null(json.FromNJson<RapidStreamerChannelMetadata>(), nameof(ChannelMetadata),
                        "An error has occured while Channel metadata request process");
                    ChannelStatus = RapidStreamerChannelState.HasMetadata;
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e, null);
                    ChannelStatus = RapidStreamerChannelState.HasError;
                    throw;
                }
            }
            catch (Exception exception)
            {
                ChannelStatus = RapidStreamerChannelState.HasError;
                Logger.Log(LogLevel.Error, exception, null);
                throw;
            }
        }

        public override Task RequestSubscriptionAsync(RapidStreamerSubscriptionRequest request, CancellationToken _ = default)
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
                            await RapidStreamerInfiniteDataStreamConnection.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken: CancellationToken.None);
                        var chunkedEncodingReadStream = await httpResponseMessage.Content.ReadAsStreamAsync(CancellationToken.None);
                        using StreamReader streamReader = new(chunkedEncodingReadStream);
                        while (!streamReader.EndOfStream &&
                               RapidStreamerConnection.ConnectionState is RapidStreamerConnectionState.Connecting or RapidStreamerConnectionState.Open
                                   or RapidStreamerConnectionState.HasError)
                        {
                            var line = await streamReader.ReadLineAsync(CancellationToken.None);
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                count++;

                                if (count == 2)
                                    AddSubscription(request);

                                await RapidStreamerInfiniteDataStreamConnection.AddMessageAsync(line, cancellationToken: CancellationToken.None);
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

        public override Task RequestUnsubscribeAsync(RapidStreamerUnsubscribeRequest request, CancellationToken cancellationToken = default)
            => RapidStreamerConnection.DisconnectAsync(cancellationToken);
    }
}