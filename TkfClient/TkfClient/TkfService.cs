using Google.Protobuf.WellKnownTypes;
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
    public class TkfService : BackgroundService
    {
        private readonly ILogger<TkfService> logger;
        private readonly InvestApiClient investApiClient;
        private readonly int interval = 1;
        public TkfService(ILogger<TkfService> logger, InvestApiClient investApiClient) 
        {
            this.logger = logger;
            this.investApiClient = investApiClient;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DateTime lastSync = new DateTime(2023,10,16,10,0,0).ToUniversalTime();
            try
            {
                var accounts = await investApiClient.Users.GetAccountsAsync(stoppingToken);

                if (accounts == null || accounts.Accounts.Count == 0)
                {
                    throw new Exception("empty accounts");
                }

                var acc = accounts.Accounts[0];

                var myPortfolio = await investApiClient.Operations.GetPortfolioAsync(new PortfolioRequest
                {
                    AccountId = acc.Id,
                    Currency = PortfolioRequest.Types.CurrencyRequest.Rub
                }, null, null, stoppingToken);

                if (myPortfolio == null || myPortfolio.Positions.Count == 0)
                {
                    throw new Exception("epmty profile positions");
                }

                // Получение списка всех Акций.
                var allShares = await investApiClient.Instruments.SharesAsync(stoppingToken);
                var monitoringShares = allShares.Instruments.Where(s => s.Currency == "rub");
                // Получение списка всех Облигаций
                var allBonds = investApiClient.Instruments.BondsAsync(stoppingToken);


                while (!stoppingToken.IsCancellationRequested)
                {
                    this.logger.LogWarning(lastSync.ToString());
                    foreach (var s in monitoringShares)
                    {
                        var candelRequest = new GetCandlesRequest()
                        {
                            InstrumentId = s.Uid,
                            Interval = CandleInterval._1Min,
                            From = lastSync.ToTimestamp(),
                            To = lastSync.AddMinutes(this.interval).ToTimestamp(),
                        };
                        var shareCandles = await investApiClient.MarketData.GetCandlesAsync(candelRequest, null, null, stoppingToken);
                        foreach (var candel in shareCandles.Candles)
                        {
                            var msg = $"{candel.Time.ToDateTime()} {s.Isin} {s.Name} O: {(decimal)candel.Open} L: {(decimal)candel.Low} H: {(decimal)candel.High} C: {(decimal)candel.Close} V: {candel.Volume}";
                            this.logger.LogInformation(msg);
                        }                        
                    }

                    lastSync = lastSync.AddMinutes(this.interval);

                    await Task.Delay(TimeSpan.FromMinutes(this.interval), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}
