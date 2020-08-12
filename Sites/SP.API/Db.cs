using Microsoft.EntityFrameworkCore;
using SP.Models;

namespace SP.API
{
    public class Db : DbContext
    {
        public DbSet<Blocks> Blocks { get; set; }
        public DbSet<LoginAttempts> LoginAttempts { get; set; }
        public DbSet<StatisticsBlocks> StatisticsBlocks { get; set; }
        public static bool EnsureCreated;

        public Db(DbContextOptions options)
            : base(options)
        {
            if (EnsureCreated)
            {
                return;
            }

            base.Database.EnsureCreated();
            EnsureCreated = true;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
        }
    }
}
