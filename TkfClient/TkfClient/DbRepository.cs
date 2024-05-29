using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TkfClient.Models;

namespace TkfClient
{
    public class DbRepository: IDisposable
    {
        private readonly IDbContextFactory<AppContext> dbContextFactory;
        private readonly AppContext ctx;
        private bool disposedValue;

        public DbRepository(IDbContextFactory<AppContext> dbContextFactory) { 
            this.dbContextFactory = dbContextFactory;
            this.ctx = this.dbContextFactory.CreateDbContext();
        }
        public void SaveCandle(CandleSync candle)
        {
            var dbCandle = ctx.Candles.FirstOrDefault(c => c.Uid == candle.Uid && c.Time == candle.Time);
            if (dbCandle == null)
            {
                ctx.Candles.Add(candle);
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
            }
            ctx.SaveChanges();
        }
        public List<Share> GetShares() => this.ctx.Shares.ToList();
        

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this.ctx.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DbRepository()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
