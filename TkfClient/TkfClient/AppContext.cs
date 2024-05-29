using Microsoft.EntityFrameworkCore;
using TkfClient.Models;

namespace TkfClient
{
    public class AppContext : DbContext
    {
        public AppContext(DbContextOptions<AppContext> dbContextOptions) 
            : base(dbContextOptions)
        { }
        public DbSet<CandleSync> Candles { get ; set; }

        public DbSet<Share> Shares { get ; set; }
    }
}
