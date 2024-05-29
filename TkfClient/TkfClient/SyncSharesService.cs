using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.InvestApi;

namespace TkfClient
{
    public class SyncSharesService : BackgroundService
    {
        private readonly ILogger<SyncSharesService> logger;
        private readonly InvestApiClient investApiClient;
        private readonly IDbContextFactory<AppContext> dbContextFactory;

        public SyncSharesService(ILogger<SyncSharesService> logger, InvestApiClient investApiClient, IDbContextFactory<AppContext> dbContextFactory)
        {
            this.logger = logger;
            this.investApiClient = investApiClient;
            this.dbContextFactory = dbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                this.logger.LogInformation($"Start sync Shares...");
                try
                {
                    var allShares = await investApiClient.Instruments.SharesAsync(stoppingToken);
                    var ruShares = allShares.Instruments.Where(s => s.Currency == "rub");

                    if (!ruShares.Any())
                    {
                        throw new Exception("Empty Shares");
                    }

                    using(AppContext ctx = this.dbContextFactory.CreateDbContext())
                    {
                        var syncShares = ruShares.Select(x => new Models.Share { Currency = x.Currency, Isin = x.Isin, Lot = x.Lot, Name = x.Name, Ticker = x.Ticker, Uid = x.Uid });
                        foreach (var s in syncShares)
                        {
                            var dbShare = await ctx.Shares.FirstOrDefaultAsync(x => x.Uid == s.Uid);
                            if (dbShare == null)
                            {
                                ctx.Shares.Add(s);
                                this.logger.LogInformation($"Add new Share: {s.Uid} {s.Isin} {s.Name} {s.Currency} {s.Lot} {s.Ticker}");                                
                            }
                            else if(!EqualShares(dbShare, s))
                            {
                                dbShare.Lot = s.Lot;
                                dbShare.Name = s.Name;
                                dbShare.Ticker = s.Ticker;
                                dbShare.Currency = s.Currency;
                                dbShare.Isin = s.Isin;
                                this.logger.LogInformation($"Update Share (${s.Uid}): {s.Isin}: {dbShare.Isin}  {s.Name}: {dbShare.Name} {s.Currency}: {dbShare.Currency} {s.Lot}: {dbShare.Lot} {s.Ticker}: {dbShare.Ticker}");
                            }
                            await ctx.SaveChangesAsync(stoppingToken);
                        }
                        this.logger.LogInformation($"Shares synced: {syncShares.Count()}");
                    }

                    await Task.Delay(60000 * 60, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, ex);
                }
            }
        }
        private bool EqualShares(Models.Share dbShare, Models.Share share)
        {
            if (dbShare == null || share == null) return false;
            if (dbShare.Name != share.Name) return false;
            if (dbShare.Isin != share.Isin) return false;
            if (dbShare.Currency != share.Currency) return false;
            if (dbShare.Lot != share.Lot) return false;
            if (dbShare.Ticker != share.Ticker) return false;
            return true;
        }
    }
}
