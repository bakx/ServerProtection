using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SP.Api.Models;
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
		public override async Task<GetLoginAttemptsResponse> GetLoginAttempts(GetLoginAttemptsRequest request,
			ServerCallContext context)
		{
			log.LogDebug(
				$"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(GetLoginAttempts)}.");

			// Open handle to database
			await using Db database = new Db(db);

			// Determine if IP Range block is enabled.
			if (request.DetectIPRange)
			{
				// Match on the first 3 blocks
				int rangeCount = database.LoginAttempts
					.Where(l =>
						l.IpAddress1 == Convert.ToByte(request.LoginAttempts.IpAddress1) &&
						l.IpAddress2 == Convert.ToByte(request.LoginAttempts.IpAddress2) &&
						l.IpAddress3 == Convert.ToByte(request.LoginAttempts.IpAddress3))
					.Count(l => l.EventDate > request.FromTime.ToDateTime());

				return new GetLoginAttemptsResponse{ Result = rangeCount };
			}

			int count = database.LoginAttempts
				.Where(l => l.IpAddress == request.LoginAttempts.IpAddress)
				.Count(l => l.EventDate > request.FromTime.ToDateTime());

			return new GetLoginAttemptsResponse { Result = count };
		}

		/// <summary>
		/// </summary>
		/// <param name="request"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public override async Task<AddLoginAttemptResponse> AddLoginAttempt(AddLoginAttemptRequest request,
			ServerCallContext context)
		{
			log.LogDebug(
				$"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(AddLoginAttempt)}.");

			// Open handle to database
			await using Db database = new Db(db);

			SP.Models.LoginAttempts loginAttempt = new SP.Models.LoginAttempts
			{
				Id = request.LoginAttempts.Id,
				IpAddress = request.LoginAttempts.IpAddress,
				IpAddress1 = Convert.ToByte(request.LoginAttempts.IpAddress1),
				IpAddress2 = Convert.ToByte(request.LoginAttempts.IpAddress2),
				IpAddress3 = Convert.ToByte(request.LoginAttempts.IpAddress3),
				IpAddress4 = Convert.ToByte(request.LoginAttempts.IpAddress4),
				Details = request.LoginAttempts.Details,
				EventDate = request.LoginAttempts.EventDate.ToDateTime()
			};

			await database.LoginAttempts.AddAsync(loginAttempt);
			bool result = await database.SaveChangesAsync() > 0;

			return new AddLoginAttemptResponse
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
				$"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(GetUnblocks)}. Parameters: minutes {request.Minutes} ");

			// Open handle to database
			await using Db database = new Db(db);

			List<Blocks> blockedEntries = database.Blocks.Where(b => b.IsBlocked == 1)
				.ToListAsync().Result.Where(b =>
					b.Date < DateTime.Now.Subtract(new TimeSpan(0, request.Minutes, 0)) &&
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
					Date =  Timestamp.FromDateTime(DateTime.SpecifyKind(blocks.Date, DateTimeKind.Utc)),
					FirewallRuleName = blocks.FirewallRuleName,
					IsBlocked = ByteString.CopyFrom(blocks.IsBlocked == 0 ? "0" : "1", Encoding.Unicode)
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
			log.LogDebug($"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(AddBlock)}.");

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
				Date = request.Blocks.Date.ToDateTime(),
				FirewallRuleName = request.Blocks.FirewallRuleName,
				IsBlocked = request.Blocks.IsBlocked.ToByteArray()[0]
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
			log.LogDebug($"Received call from {context.GetHttpContext().Connection.RemoteIpAddress} to {nameof(UpdateBlock)}.");

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
			blocks.Date = request.Blocks.Date.ToDateTime();
			blocks.Hostname = request.Blocks.Hostname;
			blocks.Details = request.Blocks.Details;
			blocks.IpAddress = request.Blocks.IpAddress;
			blocks.FirewallRuleName = request.Blocks.FirewallRuleName;
			blocks.IsBlocked = request.Blocks.IsBlocked.ToByteArray()[0];

			bool result = await database.SaveChangesAsync() > 0;

			return new UpdateBlockResponse
			{
				Result = result
			};
		}
	}
}