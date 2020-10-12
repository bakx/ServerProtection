using Microsoft.EntityFrameworkCore;
using SP.Models;

namespace SP.Api.Service
{
	public class Db : DbContext
	{
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

		public DbSet<Blocks> Blocks { get; set; }
		public DbSet<LoginAttempts> LoginAttempts { get; set; }
		public DbSet<StatisticsBlocks> StatisticsBlocks { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
		}
	}
}