using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly ILogger<StreamService> logger;
        private readonly DbRepository dbRepository;
        private readonly InvestApiClient investApiClient;
        private int retryCount = 0;

        public StreamService(ILogger<StreamService> logger, DbRepository dbRepository, InvestApiClient investApiClient) 
        {
            this.logger = logger;
            this.dbRepository = dbRepository;
            this.investApiClient = investApiClient;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var shares = this.dbRepository.GetShares();

            var scr = new SubscribeCandlesRequest { SubscriptionAction = SubscriptionAction.Subscribe };
            foreach (var s in shares)
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

            while (!cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation($"Restart Stream count: ${retryCount}");
                // Создаем стрим
                var stream = this.investApiClient.MarketDataStream.MarketDataStream();
                try
                {
                    await stream.RequestStream.WriteAsync(request, cancellationToken);
                    // Читаем ответы
                    var startTime = DateTime.Now;
                    long syncs = 0;
                    await foreach (var response in stream.ResponseStream.ReadAllAsync(cancellationToken))
                    {
                        if (response != null)
                        {
                            // Получили свечи
                            if (response.Candle != null)
                            {
                                var candle = response.Candle;
                                var share = shares.FirstOrDefault(s => s.Uid == candle.InstrumentUid);
                                if (share == null)
                                {
                                    logger.LogWarning($"Не удалось найти инструмент {candle.InstrumentUid} в списке на синхронизацию");
                                    continue;
                                }
                                try
                                {
                                    var syncCandle = new CandleSync
                                    {
                                        Uid = candle.InstrumentUid,
                                        Time = candle.Time.ToDateTime(),
                                        Open = candle.Open,
                                        Close = candle.Close,
                                        High = candle.High,
                                        Low = candle.Low,
                                        Volume = candle.Volume,
                                    };
                                    this.dbRepository.SaveCandle(syncCandle);
                                    syncs++;
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, ex.Message);
                                    continue;
                                }

                                var msg = $"{candle.Time.ToDateTime()} {share.Isin} {share.Name} O: {(decimal)candle.Open} L: {(decimal)candle.Low} H: {(decimal)candle.High} C: {(decimal)candle.Close} V: {candle.Volume}";
                                //this.logger.LogInformation(msg);
                                
                                if (DateTime.Now - startTime >= TimeSpan.FromMinutes(1))
                                {
                                    this.logger.LogInformation($"syncs: {syncs}");
                                    startTime = DateTime.Now;
                                    syncs = default;
                                }
                            }
                            else
                            {
                                logger.LogWarning("Candle is empty");
                            }
                        }
                        else
                        {
                            logger.LogWarning("Response is empty");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
                // Задержка перед последущим запуском в 10с.
                await Task.Delay(10000, cancellationToken);
                retryCount++;
            }
        }
    }
}
