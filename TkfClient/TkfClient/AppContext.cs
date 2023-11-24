using Microsoft.EntityFrameworkCore;
using TkfClient.Models;

namespace TkfClient
{
    internal class AppContext : DbContext
    {
        public AppContext(DbContextOptions<AppContext> dbContextOptions) 
            : base(dbContextOptions)
        { }
        public DbSet<CandleSync> Candles { get ; set; }
    }
}
