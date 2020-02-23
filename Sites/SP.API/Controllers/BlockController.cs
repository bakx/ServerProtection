﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SP.Models;

namespace SP.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlockController : ControllerBase
    {
        private readonly ILogger<BlockController> log;

        public BlockController(ILogger<BlockController> log)
        {
            this.log = log;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(GetUnblocks))]
        public async Task<List<Blocks>> GetUnblocks(int minutes = 30)
        {
            log.LogDebug($"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetUnblocks)}. Parameters: minutes {minutes} ");

            // Open handle to database
            await using Db db = new Db();

            return db.Blocks.Where(b => b.IsBlocked == 1)
                .ToListAsync().Result.Where(b =>
                    b.Date < DateTime.Now.Subtract(new TimeSpan(0, minutes, 0)) &&
                    b.IsBlocked == 1
                ).ToList();
        }

        [HttpPost]
        [Route(nameof(AddBlock))]
        public async Task<bool> AddBlock(Blocks block)
        {
            log.LogDebug($"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(AddBlock)}.");

            // Open handle to database
            await using Db db = new Db();
            db.Blocks.Add(block);
            return await db.SaveChangesAsync() > 0;
        }

        [HttpPost]
        [Route(nameof(UpdateBlock))]
        public async Task<bool> UpdateBlock(Blocks block)
        {
            log.LogDebug($"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(UpdateBlock)}.");

            // Open handle to database
            await using Db db = new Db();

            Blocks blocks = db.Blocks.Single(b => b.Id == block.Id);

            // If the entry cannot be found, ignore the update
            if (blocks == null)
            {
                return false;
            }
            
            // Overwrite block details
            blocks.City = block.City;
            blocks.Country = block.Country;
            blocks.Date = block.Date;
            blocks.Hostname = block.Hostname;
            blocks.Details = block.Details;
            blocks.IpAddress = block.IpAddress;
            blocks.IpAddress1 = block.IpAddress1;
            blocks.IpAddress2 = block.IpAddress2;
            blocks.IpAddress3 = block.IpAddress3;
            blocks.IpAddress4 = block.IpAddress4;
            blocks.FirewallRuleName = block.FirewallRuleName;
            blocks.IsBlocked = block.IsBlocked;

            return await db.SaveChangesAsync() > 0;
        }
    }
}