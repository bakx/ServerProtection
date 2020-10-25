using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SP.Api.Models;
using SP.Models.Enums;
using Blocks = SP.Models.Blocks;

namespace SP.Api.Service
{
	internal class ApiService : ApiServices.ApiServicesBase
	{
		private readonly DbContextOptions<Db> db;

		// Configuration object

		private readonly ILogger log;

		/// <summary>
		/// </summary>
		/// <param name="log"></param>
		/// <param name="db"></param>
		public ApiService(ILogger<ApiService> log, DbContextOptions<Db> db)
		{
			this.log = log;
			this.db = db;
		}

		/// <summary>
		/// </summary>
		/// <param name="request"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public override async Task<GetAccessAttemptsResponse> GetLoginAttempts(GetAccessAttemptsRequest request,
			ServerCallContext context)
		{
			log.LogDebug(
				$"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(GetLoginAttempts)} for IP {request.AccessAttempt.IpAddress}");

			// Open handle to database
			await using Db database = new Db(db);

			// Determine if IP Range block is enabled.
			if (request.DetectIPRange)
			{
				// Match on the first 3 blocks
				int rangeCount = database.AccessAttempts
					.Where(l =>
						l.IpAddress1 == Convert.ToByte(request.AccessAttempt.IpAddress1) &&
						l.IpAddress2 == Convert.ToByte(request.AccessAttempt.IpAddress2) &&
						l.IpAddress3 == Convert.ToByte(request.AccessAttempt.IpAddress3))
					.Count(l => l.EventDate > request.FromTime.ToDateTime());

				return new GetAccessAttemptsResponse{ Result = rangeCount };
			}

			int count = database.AccessAttempts
				.Where(l => l.IpAddress == request.AccessAttempt.IpAddress)
				.Count(l => l.EventDate > request.FromTime.ToDateTime());

			return new GetAccessAttemptsResponse { Result = count };
		}

		/// <summary>
		/// </summary>
		/// <param name="request"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public override async Task<AddAccessAttemptResponse> AddLoginAttempt(AddAccessAttemptRequest request,
			ServerCallContext context)
		{
			log.LogDebug(
				$"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(AddLoginAttempt)} for IP {request.AccessAttempts.IpAddress}");

			// Open handle to database
			await using Db database = new Db(db);

			SP.Models.AccessAttempts accessAttempt = new SP.Models.AccessAttempts
			{
				Id = request.AccessAttempts.Id,
				IpAddress = request.AccessAttempts.IpAddress,
				IpAddress1 = Convert.ToByte(request.AccessAttempts.IpAddress1),
				IpAddress2 = Convert.ToByte(request.AccessAttempts.IpAddress2),
				IpAddress3 = Convert.ToByte(request.AccessAttempts.IpAddress3),
				IpAddress4 = Convert.ToByte(request.AccessAttempts.IpAddress4),
				Details = request.AccessAttempts.Details,
				EventDate = request.AccessAttempts.EventDate.ToDateTime(),
				AttackType = (AttackType) request.AccessAttempts.AttackType,
				Custom1 = request.AccessAttempts.Custom1,
				Custom2 = request.AccessAttempts.Custom2,
				Custom3 = request.AccessAttempts.Custom3
			};

			await database.AccessAttempts.AddAsync(accessAttempt);
			bool result = await database.SaveChangesAsync() > 0;

			return new AddAccessAttemptResponse
			{
				Result = result
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public override async Task<GetUnblocksResponse> GetUnblocks(GetUnblocksRequest request,
			ServerCallContext context)
		{
			log.LogDebug(
				$"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(GetUnblocks)}. Parameters: minutes {request.Minutes} date {request.Date} ");

			// Check if a date is passed
			DateTime date = request.Date != null ? request.Date.ToDateTime() : DateTime.Now;

			// Open handle to database
			await using Db database = new Db(db);

			List<Blocks> blockedEntries = database.Blocks.Where(b => b.IsBlocked == 1)
				.ToListAsync().Result.Where(b =>
					b.EventDate < date.Subtract(new TimeSpan(0, request.Minutes, 0)) &&
					b.IsBlocked == 1
				).ToList();

			// Prepare response
			GetUnblocksResponse result = new GetUnblocksResponse();

			// Convert models
			result.Blocks.AddRange( new RepeatedField<Models.Blocks>
			{
				blockedEntries.Select(blocks => new Models.Blocks
				{
					Id = blocks.Id,
					IpAddress = blocks.IpAddress,
					Hostname = blocks.Hostname,
					Country = blocks.Country ?? "",
					City = blocks.City ?? "",
					ISP = blocks.ISP ?? "",
					Details = blocks.Details,
					Date =  Timestamp.FromDateTime(DateTime.SpecifyKind(blocks.EventDate, DateTimeKind.Utc)),
					FirewallRuleName = blocks.FirewallRuleName,
					IsBlocked = blocks.IsBlocked,
					AttackType = (int) blocks.AttackType
				})
			});

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public override async Task<AddBlockResponse> AddBlock(AddBlockRequest request,
			ServerCallContext context)
		{
			log.LogDebug($"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(AddBlock)} for IP {request.Blocks.IpAddress}");

			// Open handle to database
			await using Db database = new Db(db);

			Blocks block = new Blocks
			{
				Id = request.Blocks.Id,
				IpAddress = request.Blocks.IpAddress,
				Hostname = request.Blocks.Hostname,
				Country = request.Blocks.Country ?? "",
				City = request.Blocks.City ?? "",
				ISP = request.Blocks.ISP ?? "",
				EventDate = request.Blocks.Date.ToDateTime(),
				Details = request.Blocks.Details,
				FirewallRuleName = request.Blocks.FirewallRuleName,
				IsBlocked = (byte) request.Blocks.IsBlocked,
				AttackType = (AttackType) request.Blocks.AttackType
			};

			await database.Blocks.AddAsync(block);
			bool result = await database.SaveChangesAsync() > 0;

			return new AddBlockResponse
			{
				Result = result
			};
		}

		public override async Task<UpdateBlockResponse> UpdateBlock(UpdateBlockRequest request,
			ServerCallContext context)
		{
			log.LogDebug($"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(UpdateBlock)} for IP {request.Blocks.IpAddress}");

			// Open handle to database
			await using Db database = new Db(db);

			Blocks blocks = database.Blocks.SingleOrDefault(b => b.Id == request.Blocks.Id);

			// If the entry cannot be found, ignore the update
			if (blocks == null)
			{
				return new UpdateBlockResponse
				{
					Result = false
				};
			}

			// Overwrite block details
			blocks.City = request.Blocks.City;
			blocks.Country = request.Blocks.Country;
			blocks.EventDate = request.Blocks.Date.ToDateTime();
			blocks.Hostname = request.Blocks.Hostname;
			blocks.Details = request.Blocks.Details;
			blocks.IpAddress = request.Blocks.IpAddress;
			blocks.FirewallRuleName = request.Blocks.FirewallRuleName;
			blocks.IsBlocked = (byte) request.Blocks.IsBlocked;
			blocks.AttackType = (AttackType) request.Blocks.AttackType;

			bool result = await database.SaveChangesAsync() > 0;

			return new UpdateBlockResponse
			{
				Result = result
			};
		}

		/// <summary>
		/// </summary>
		/// <param name="request"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public override async Task<StatisticsUpdateBlocksResponse> StatisticsUpdateBlocks(StatisticsUpdateBlocksRequest request,
			ServerCallContext context)
		{
			log.LogDebug($"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(StatisticsUpdateBlocks)}.");

			// Open handle to database
			await using Db database = new Db(db);

			// Determine if this country has been blocked before
			SP.Models.StatisticsBlocks blocks = database.StatisticsBlocks
				.FirstOrDefault(s =>
					s.Country == request.Blocks.Country && 
					s.City == request.Blocks.City && 
					s.ISP == request.Blocks.ISP);

			if (blocks != null)
			{
				blocks.Attempts++;
			}
			else
			{
				SP.Models.StatisticsBlocks statisticsBlocks = new SP.Models.StatisticsBlocks
				{
					Country = request.Blocks.Country,
					ISP = request.Blocks.ISP,
					City = request.Blocks.City,
					Attempts = 1
				};

				await database.StatisticsBlocks.AddAsync(statisticsBlocks);
			}

			bool result = await database.SaveChangesAsync() > 0;

			return new StatisticsUpdateBlocksResponse
			{
				Result = result
			};
		}
	}
}