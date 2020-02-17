using Microsoft.EntityFrameworkCore;
using SP.Core.Models;

namespace SP.Core
{
    internal class Db : DbContext
    {
        public DbSet<Blocks> Blocks { get; set; }
        public DbSet<LoginAttempts> LoginAttempts { get; set; }
        public DbSet<StatisticsBlocks> StatisticsBlocks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
#if DEBUG
            options.UseSqlite("Data Source=Data\\sqlite.development.db");
#else
            options.UseSqlite("Data Source=Data\\sqlite.db");
#endif
        }
    }
}