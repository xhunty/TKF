using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TkfClient.Models;

namespace TkfClient
{
    internal class StreamService : BackgroundService
    {
        private readonly ILogger<TkfService> logger;
        private readonly InvestApiClient investApiClient;
        private readonly IDbContextFactory<AppContext> dbContextFactory;

        public StreamService(ILogger<TkfService> logger, InvestApiClient investApiClient, IDbContextFactory<AppContext> dbContextFactory) 
        {
            this.logger = logger;
            this.investApiClient = investApiClient;
            this.dbContextFactory = dbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                var accounts = await investApiClient.Users.GetAccountsAsync(cancellationToken);

                if (accounts == null || accounts.Accounts.Count == 0)
                {
                    throw new Exception("empty accounts");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                Environment.Exit(1);
            }

            var allShares = await investApiClient.Instruments.SharesAsync(cancellationToken);
            var monitoringShares = allShares.Instruments.Where(s => s.Currency == "rub");

            if (!monitoringShares.Any())
            {
                throw new Exception("Empty Shares");
            }

            var scr = new SubscribeCandlesRequest { SubscriptionAction = SubscriptionAction.Subscribe };

            foreach (var s in monitoringShares)
            {
                scr.Instruments.Add(new CandleInstrument
                {
                    InstrumentId = s.Uid,
                    Interval = SubscriptionInterval.OneMinute
                });
            }

            var request = new MarketDataRequest
            {
                SubscribeCandlesRequest = scr
            };

            var stream = this.investApiClient.MarketDataStream.MarketDataStream();

            await stream.RequestStream.WriteAsync(request, cancellationToken);

            await foreach(var response in stream.ResponseStream.ReadAllAsync(cancellationToken)) 
            {
                if (response != null)
                {
                    // Получили свечи
                    if (response.Candle != null)
                    {
                        var candle = response.Candle;
                        var share = monitoringShares.FirstOrDefault(s => s.Uid == candle.InstrumentUid);

                        if (share == null)
                        {
                            logger.LogWarning($"Не удалось найти инструмент {candle.InstrumentUid} в списке на синхронизацию");
                            continue;
                        }
                        try
                        {
                            await WriteCandle(candle, share, cancellationToken);
                        }
                        catch (DbUpdateException ex)
                        {
                            logger.LogError(ex, ex.Message);
                            continue;
                        }

                        var msg = $"{candle.Time.ToDateTime()} {share.Isin} {share.Name} O: {(decimal)candle.Open} L: {(decimal)candle.Low} H: {(decimal)candle.High} C: {(decimal)candle.Close} V: {candle.Volume}";

                        logger.LogInformation(msg);
                    }
                    else
                    {
                        logger.LogWarning("Empty response candle");
                    }
                }
                else
                {
                    logger.LogError("Empty response");
                }
                
            }
        }

        private async Task WriteCandle(Candle candle, Share share, CancellationToken cancellationToken)
        {
            using (AppContext context = this.dbContextFactory.CreateDbContext())
            {
                var candleDb = await context.Candles.FirstOrDefaultAsync(c => c.Uid == share.Uid && c.Time == candle.Time.ToDateTime(), cancellationToken);

                if (candleDb != null)
                {
                    candleDb.Open = (decimal)candle.Open;
                    candleDb.Close = (decimal)candle.Close;
                    candleDb.Low = (decimal)candle.Low;
                    candleDb.Volume = candle.Volume;
                    candleDb.High = (decimal)candle.High;
                }
                else
                {
                    context.Candles.Add(new CandleSync
                    {
                        Uid = share.Uid,
                        Open = (decimal)candle.Open,
                        Close = (decimal)candle.Close,
                        Low = (decimal)candle.Low,
                        High = (decimal)candle.High,
                        Volume = candle.Volume,
                        Time = candle.Time.ToDateTime(),
                        Isin = share.Isin
                    });
                }

                await context.SaveChangesAsync(cancellationToken);
            }            
        }
    }
}
