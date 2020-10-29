using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SP.Models;
using SP.Models.Enums;
using SP.Models.Statistics;

namespace SP.Api.Overview.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class StatisticsController : ControllerBase
	{
		private readonly DbContextOptions<Db> db;
		private readonly ILogger<StatisticsController> log;

		public StatisticsController(ILogger<StatisticsController> log, DbContextOptions<Db> db)
		{
			this.log = log;
			this.db = db;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetTopAttacks))]
		[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<IEnumerable<TopAttacks>> GetTopAttacks()
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetTopAttacks)}.");

			// Open handle to database
			await using Db database = new Db(db);

			// Return object
			return database.Blocks
				.Where(d => d.EventDate > DateTime.Now.AddDays(-1))
				.AsEnumerable()
				.GroupBy(s => s.AttackType)
				.Select(cl => new TopAttacks
				{
					AttackType = (int) cl.Key,
					Attempts = cl.LongCount()
				})
				.ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="top"></param>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetTopCountries))]
		[ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<IEnumerable<TopCountries>> GetTopCountries(int top = 10)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetTopCountries)}.");

			// Open handle to database
			await using Db database = new Db(db);

			// Return object
			return database.StatisticsBlocks
				.Where(s => s.Country != null)
				.AsEnumerable()
				.GroupBy(s => s.Country)
				.Select(cl => new TopCountries
				{
					Country = cl.First().Country,
					Attempts = cl.Sum(s => s.Attempts)
				})
				.OrderByDescending(s => s.Attempts)
				.Take(top)
				.ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="top"></param>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetTopCities))]
		[ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<IEnumerable<TopCities>> GetTopCities(int top = 10)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetTopCities)}.");

			// Open handle to database
			await using Db database = new Db(db);

			// Return object
			return database.StatisticsBlocks
				.Where(s => s.City != null)
				.AsEnumerable()
				.GroupBy(s => s.City)
				.Select(cl => new TopCities
				{
					City = cl.First().City.Length > 0 ? cl.First().City : "Unknown",
					Attempts = cl.Sum(s => s.Attempts)
				})
				.OrderByDescending(s => s.Attempts)
				.Take(top)
				.ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="top"></param>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetTopIps))]
		[ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<IEnumerable<TopIps>> GetTopIps(int top = 10)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetTopCities)}.");

			// Open handle to database
			await using Db database = new Db(db);

			//
			return (database.AccessAttempts
					.AsEnumerable() ?? throw new InvalidOperationException())
				.GroupBy(s => s.IpAddress)
				.Select(cl => new TopIps
				{
					IpAddress = cl.First().IpAddress,
					Attempts = cl.Count()
				})
				.OrderByDescending(s => s.Attempts)
				.Take(top)
				.ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetAttackTypesAttemptsPerHour))]
		[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<List<StatsPerHourCollection>> GetAttackTypesAttemptsPerHour()
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetAttackTypesAttemptsPerHour)}.");

			// Open handle to database
			await using Db database = new Db(db);

			// Get last NN days
			DateTime lastDays = DateTime.Now.AddDays(-1);

			// Get all type of Attack Types in time frame
			List<AttackType> attackTypes = database.AccessAttempts
				.Where(d => d.EventDate > lastDays && d.EventDate < DateTime.Now)
				.ToList()
				.GroupBy(d => d.AttackType)
				.Select(d => d.Key)
				.ToList();

			//
			List<StatsPerHourCollection> statsPerHourCollection = new List<StatsPerHourCollection>();

			// Get stats for all Attack Types
			foreach (AttackType attackType in attackTypes)
			{
				List<StatsPerHour> statsPerHours = database.AccessAttempts
					.Where(d => d.AttackType == attackType && 
					            d.EventDate > lastDays && d.EventDate < DateTime.Now)
					.ToList()
					.GroupBy(l => l.EventDate.ToString("HH"))
					.Select(d => new StatsPerHour
					{
						Key = Convert.ToInt32(d.Key),
						Attempts = d.LongCount()
					})
					.OrderBy(d => d.Key)
					.ToList();

				// Assure that all keys have a value or any charts rendered from this data might not work correctly.
				for (int i = 0; i < 24; i++)
				{
					if (statsPerHours.Any(c => c.Key == i))
					{
						continue;
					}

					statsPerHours.Insert( i, new StatsPerHour{ Key = i, Attempts = 0});
				}

				statsPerHourCollection.Add(new StatsPerHourCollection {
					Key = Convert.ToString(attackType), 
					Data = statsPerHours
				});
			}

			// Validate that all data sets have data. If not, add 0 as default
			

			return statsPerHourCollection;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetAttemptsPerDay))]
		[ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<IEnumerable<StatsPerDay>> GetAttemptsPerDay()
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetAttemptsPerDay)}.");

			// Get last month
			DateTime lastMonth = DateTime.Now.AddMonths(-1);

			// Open handle to database
			await using Db database = new Db(db);

			//
			return database.AccessAttempts
				.Where(d => d.EventDate > lastMonth && d.EventDate < DateTime.Now)
				.AsEnumerable()
				.GroupBy(l => l.EventDate.ToString("M"))
				.ToList()
				.Select(l => new StatsPerDay
				{
					Key = l.Key,
					Attempts = l.LongCount()
				})
				.OrderBy(l => DateTime.Parse(l.Key));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="days"></param>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetBlocksPerHour))]
		[ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<IEnumerable<StatsPerHour>> GetBlocksPerHour(int days = 30)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetBlocksPerHour)}.");

			// Get last NN days
			DateTime lastDays = DateTime.Now.AddDays(days * -1);

			// Open handle to database
			await using Db database = new Db(db);

//
			return database.Blocks
				.Where(d => d.EventDate > lastDays)
				.AsEnumerable()
				.OrderByDescending(l => l.Id)
				.ToList()
				.GroupBy(l => DateTime.Parse(Convert.ToString(l.EventDate, CultureInfo.InvariantCulture)).Hour)
				.Select(l => new StatsPerHour
				{
					Key = l.Key,
					Attempts = l.LongCount()
				})
				.OrderBy(l => l.Key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetBlocksPerDay))]
		[ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<IEnumerable<StatsPerHour>> GetBlocksPerDay()
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetBlocksPerDay)}.");

			// Get last month
			DateTime lastMonth = DateTime.Now.AddMonths(-1);

			// Open handle to database
			await using Db database = new Db(db);

			//
			return database.Blocks
				.Where(d => d.EventDate.Month == lastMonth.Month && d.EventDate.Year == lastMonth.Year)
				.AsEnumerable()
				.OrderByDescending(l => l.Id)
				.ToList()
				.GroupBy(l => l.EventDate.Day)
				.Select(l => new StatsPerHour
				{
					Key = l.Key,
					Attempts = l.LongCount()
				})
				.OrderBy(l => l.Key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetLatestAttempts))]
		[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<IEnumerable<AccessAttempts>> GetLatestAttempts()
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetLatestAttempts)}.");

			// Open handle to database
			await using Db database = new Db(db);

			//
			return database.AccessAttempts
				.OrderByDescending(b => b.Id)
				.Take(10)
				.ToList();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetLatestBlocks))]
		[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
		public async Task<IEnumerable<Blocks>> GetLatestBlocks()
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetLatestBlocks)}.");

			// Open handle to database
			await using Db database = new Db(db);

			//
			return database.Blocks
				.OrderByDescending(b => b.Id)
				.Take(10)
				.ToList();
		}
	}
}