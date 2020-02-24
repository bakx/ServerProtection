using System.Collections.Generic;
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
            return db.LoginAttempts
                .AsEnumerable()
                .GroupBy(s => s.IpAddress)
                .Select(cl => new TopIps
                {
                    IpAddress = cl.First().IpAddress,
                    Attempts = cl.Count()
                })
                .OrderByDescending(s => s.Attempts)
                .Take(10)
                .ToList();
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