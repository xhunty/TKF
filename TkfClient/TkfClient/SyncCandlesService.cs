using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;

namespace TkfClient
{
    public class SyncCandlesService : BackgroundService
    {
        private readonly IDbContextFactory<AppContext> dbContextFactory;
        private readonly InvestApiClient investApiClient;
        private readonly ILogger<SyncCandlesService> logger;

        private readonly DateTime startDate = new DateTime(2024, 5, 15, 22,0,0);

        public SyncCandlesService(IDbContextFactory<AppContext> dbContextFactory, InvestApiClient investApiClient, ILogger<SyncCandlesService> logger)
        {
            this.dbContextFactory = dbContextFactory;
            this.investApiClient = investApiClient;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (AppContext ctx = this.dbContextFactory.CreateDbContext())
                {
                    var instriments = await ctx.Shares.AsNoTracking().ToListAsync(stoppingToken);
                    var syncDate = startDate.ToUniversalTime();
                    int totalAddedCandles = 0;
                    int totalUpdatedCandles = 0;

                    while (syncDate < DateTime.UtcNow && !stoppingToken.IsCancellationRequested)
                    {
                        var syncDateTo = syncDate.AddDays(1);
                        foreach (var instriment in instriments)
                        {
                            logger.LogInformation($"start sync for {instriment.Ticker}...");

                            int addedCandles = 0;
                            int updatedCandles = 0;
                            GetCandlesRequest candlesRequest = new GetCandlesRequest();                            
                            candlesRequest.InstrumentId = instriment.Uid;
                            candlesRequest.Interval = CandleInterval._1Min;
                            candlesRequest.From = syncDate.ToTimestamp();
                            candlesRequest.To = syncDateTo.ToTimestamp();
                            candlesRequest.CandleSourceType = GetCandlesRequest.Types.CandleSource.Exchange;

                            var res = await this.investApiClient.MarketData.GetCandlesAsync(candlesRequest, null, null, stoppingToken);

                            if (res != null && res.Candles != null)
                            {
                                foreach (var candle in res.Candles)
                                {
                                    var dbCandle = ctx.Candles.FirstOrDefault(c => c.Uid == instriment.Uid && c.Time == candle.Time.ToDateTime());
                                    if (dbCandle == null)
                                    {
                                        ctx.Candles.Add(new Models.CandleSync
                                        {
                                            Uid = instriment.Uid,
                                            Open = candle.Open,
                                            Close = candle.Close,
                                            High = candle.High,
                                            Low = candle.Low,
                                            Volume = candle.Volume,
                                            Time = candle.Time.ToDateTime(),
                                        });
                                        addedCandles++;
                                        totalAddedCandles++;
                                    }
                                    else if (dbCandle.Open != candle.Open ||
                                        dbCandle.Low != candle.Low ||
                                        dbCandle.Close != candle.Close ||
                                        dbCandle.High != candle.High ||
                                        dbCandle.Volume != candle.Volume)
                                    {
                                        dbCandle.Open = candle.Open;
                                        dbCandle.Low = candle.Low;
                                        dbCandle.Close = candle.Close;
                                        dbCandle.High = candle.High;
                                        dbCandle.Volume = candle.Volume;
                                        updatedCandles++;
                                        totalUpdatedCandles++;
                                    }
                                }
                            }
                            await ctx.SaveChangesAsync(stoppingToken);
                            logger.LogInformation($"from {syncDate} to {syncDateTo}: added {addedCandles} updated {updatedCandles}");
                            await Task.Delay(100);
                        }
                        syncDate = syncDateTo;
                    }
                    logger.LogInformation($"end sync: added {totalAddedCandles} updated {totalUpdatedCandles}");

                }
            }
        }
    }
}
