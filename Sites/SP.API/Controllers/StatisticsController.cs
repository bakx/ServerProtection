using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SP.Models;
using SP.Models.Statistics;

namespace SP.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ILogger<StatisticsController> log;

        public StatisticsController(ILogger<StatisticsController> log)
        {
            this.log = log;
        }

        [HttpGet]
        [Route(nameof(GetTopCountries))]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IEnumerable<TopCountries>> GetTopCountries(int top = 10)
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetTopCountries)}.");

            // Open handle to database
            await using Db db = new Db();

            // Return object
            return db.StatisticsBlocks
                .Where(s => s.Country != null)
                .AsEnumerable()
                .GroupBy(s => s.Country)
                .Select(cl => new TopCountries
                {
                    Country = cl.First().Country,
                    Attempts = cl.Sum(s => s.Attempts),
                })
                .OrderByDescending(s => s.Attempts)
                .Take(top)
                .ToList();
        }

        [HttpGet]
        [Route(nameof(GetTopCities))]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IEnumerable<TopCities>> GetTopCities(int top = 10)
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetTopCities)}.");

            // Open handle to database
            await using Db db = new Db();

            // Return object
            return db.StatisticsBlocks
                .Where(s => s.City != null)
                .AsEnumerable()
                .GroupBy(s => s.City)
                .Select(cl => new TopCities
                {
                    City = cl.First().City.Length > 0 ? cl.First().City : "Unknown",
                    Attempts = cl.Sum(s => s.Attempts),
                })
                .OrderByDescending(s => s.Attempts)
                .Take(top)
                .ToList();
        }

        [HttpGet]
        [Route(nameof(GetTopIps))]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IEnumerable<TopIps>> GetTopIps(int top = 10)
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetTopCities)}.");

            // Open handle to database
            await using Db db = new Db();

            //
            return (db.LoginAttempts
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

        [HttpGet]
        [Route(nameof(GetAttemptsPerHour))]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IEnumerable<StatsPerHour>> GetAttemptsPerHour(int days = 30)
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetAttemptsPerHour)}.");

            // Open handle to database
            await using Db db = new Db();

            // Get last NN days
            DateTime lastDays = DateTime.Now.AddDays(days * -1);

            //
            return db.LoginAttempts
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

        [HttpGet]
        [Route(nameof(GetAttemptsPerDay))]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IEnumerable<StatsPerHour>> GetAttemptsPerDay(int top = 2500)
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetAttemptsPerDay)}.");

            // Open handle to database
            await using Db db = new Db();

            // Get last month
            DateTime lastMonth = DateTime.Now.AddMonths(-1);

            //
            return db.LoginAttempts
                .Where(d => d.EventDate.Month == lastMonth.Month && d.EventDate.Year == lastMonth.Year)
                .AsEnumerable()
                .OrderByDescending(l => l.Id)
                .Take(top)
                .ToList()
                .GroupBy(l => DateTime.Parse(Convert.ToString(l.EventDate, CultureInfo.InvariantCulture)).Hour)
                .Select(l => new StatsPerHour
                {
                    Key = l.Key,
                    Attempts = l.LongCount()
                })
                .OrderBy(l => l.Key);
        }

        [HttpGet]
        [Route(nameof(GetBlocksPerHour))]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IEnumerable<StatsPerHour>> GetBlocksPerHour(int days = 30)
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetBlocksPerHour)}.");

            // Open handle to database
            await using Db db = new Db();

            // Get last NN days
            DateTime lastDays = DateTime.Now.AddDays(days * -1);

            //
            return db.Blocks
                .Where(d => d.Date > lastDays)
                .AsEnumerable()
                .OrderByDescending(l => l.Id)
                .ToList()
                .GroupBy(l => DateTime.Parse(Convert.ToString(l.Date, CultureInfo.InvariantCulture)).Hour)
                .Select(l => new StatsPerHour
                {
                    Key = l.Key,
                    Attempts = l.LongCount()
                })
                .OrderBy(l => l.Key);
        }

        [HttpGet]
        [Route(nameof(GetBlocksPerDay))]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IEnumerable<StatsPerHour>> GetBlocksPerDay()
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetBlocksPerDay)}.");

            // Open handle to database
            await using Db db = new Db();

            // Get last month
            DateTime lastMonth = DateTime.Now.AddMonths(-1);

            //
            return db.Blocks
                .Where(d => d.Date.Month == lastMonth.Month && d.Date.Year == lastMonth.Year)
                .AsEnumerable()
                .OrderByDescending(l => l.Id)
                .ToList()
                .GroupBy(l => DateTime.Parse(Convert.ToString(l.Date, CultureInfo.InvariantCulture)).Day)
                .Select(l => new StatsPerHour
                {
                    Key = l.Key,
                    Attempts = l.LongCount()
                })
                .OrderBy(l => l.Key);
        }

        [HttpPost]
        [Route(nameof(UpdateBlock))]
        public async Task<bool> UpdateBlock(Blocks block)
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(UpdateBlock)}.");

            // Open handle to database
            await using Db db = new Db();

            // Determine if this country has been blocked before
            StatisticsBlocks blocks = db.StatisticsBlocks.FirstOrDefault(s =>
                s.Country == block.Country && s.City == block.City && s.ISP == block.ISP);

            if (blocks != null)
            {
                blocks.Attempts++;
            }
            else
            {
                StatisticsBlocks statisticsBlocks = new StatisticsBlocks
                {
                    Country = block.Country,
                    ISP = block.ISP,
                    City = block.City,
                    Attempts = 1
                };

                db.StatisticsBlocks.Add(statisticsBlocks);
            }

            // Save changes
            return await db.SaveChangesAsync() > 0;
        }
    }
}