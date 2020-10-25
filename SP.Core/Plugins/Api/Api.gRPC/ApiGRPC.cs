using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Api.Models;
using SP.Models.Enums;
using SP.Plugins;
using Blocks = SP.Models.Blocks;

namespace Plugins
{
	public class ApiGrpc : PluginBase, IApiHandler
	{
		private string serverHost;
		private int serverPort;
		private string certificatePath;
		private string certificatePassword;

		// Diagnostics
		private ILogger log;

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override Task<bool> Initialize(PluginOptions options)
		{
			try
			{
				// Initiate the configuration
				IConfigurationRoot config = new ConfigurationBuilder()
					.SetBasePath(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
#if DEBUG
					.AddJsonFile("appSettings.development.json", false, true)
#else
                    .AddJsonFile("appSettings.json", false, true)
#endif
					.AddJsonFile("logSettings.json", false, true)
					.Build();

				log = new LoggerConfiguration()
					.ReadFrom.Configuration(config)
					.CreateLogger()
					.ForContext(typeof(ApiGrpc));

				// Assign config variables
				serverHost = config.GetSection("Host").Get<string>();
				serverPort = config.GetSection("Port").Get<int>();
				certificatePath = config.GetSection("CertificatePath").Get<string>();
				certificatePassword = config.GetSection("CertificatePassword").Get<string>();

				// Diagnostics
				log.Information("Plugin initialized");

				return Task.FromResult(true);
			}
			catch (Exception e)
			{
				if (log == null)
				{
					Console.WriteLine(e);
				}
				else
				{
					log.Error(e.Message);
				}

				return Task.FromResult(false);
			}
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override Task<bool> Configure()
		{
			// This plug-in does not use the Configure functionality.
			// To keep the logs consistent, output the 'completed' message regardless.

			if (log == null)
			{
				Console.WriteLine("Completed Configuration stage");
			}
			else
			{
				log.Information("Completed Configuration stage");
			}

			return Task.FromResult(true);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		private GrpcChannel GetChannel()
		{
			// Load certificate
			X509Certificate2 cert = new X509Certificate2(certificatePath, certificatePassword);

			HttpClientHandler handler = new HttpClientHandler();
			handler.ClientCertificates.Add(cert);

			handler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

			GrpcChannel grpcChannel = GrpcChannel.ForAddress($"{serverHost}:{serverPort}", new GrpcChannelOptions
			{
				HttpHandler = handler
			});

			return grpcChannel;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="minutes"></param>
		/// <returns></returns>
		public async Task<List<Blocks>> GetUnblock(int minutes)
		{
			// Diagnostics
			log.Debug($"{nameof(GetUnblock)} called with param {minutes}");

			// Get connection
			GrpcChannel channel = GetChannel();
			ApiServices.ApiServicesClient client = new ApiServices.ApiServicesClient(channel);

			// Make gRPC request
			GetUnblocksResponse response = await client.GetUnblocksAsync(
				new GetUnblocksRequest
				{
					Minutes = minutes,
					Date = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc))
				});

			// Diagnostics
			log.Debug($"{nameof(GetUnblock)} received server response: {response.Blocks.Count} items");

			// Clean up
			await channel.ShutdownAsync();

			// Convert models.
			return response.Blocks.Select(blocks => new Blocks
				{
					Id = blocks.Id,
					IpAddress = blocks.IpAddress,
					IpAddress1 = Convert.ToByte(blocks.IpAddress.Split(".")[0]),
					IpAddress2 = Convert.ToByte(blocks.IpAddress.Split(".")[1]),
					IpAddress3 = Convert.ToByte(blocks.IpAddress.Split(".")[2]),
					IpAddress4 = Convert.ToByte(blocks.IpAddress.Split(".")[3]),
					Hostname = blocks.Hostname,
					Country = blocks.Country,
					City = blocks.City,
					ISP = blocks.ISP,
					Details = blocks.Details,
					EventDate = blocks.Date.ToDateTime(),
					FirewallRuleName = blocks.FirewallRuleName,
					IsBlocked = (byte) blocks.IsBlocked,
					AttackType = (AttackType) blocks.AttackType
				})
				.ToList();
		}

		/// <summary>
		/// </summary>
		/// <param name="blocks"></param>
		/// <returns></returns>
		public async Task<bool> AddBlock(Blocks blocks)
		{
			// Diagnostics
			log.Debug($"{nameof(AddBlock)} called for {blocks.IpAddress}");

			// Get connection
			GrpcChannel channel = GetChannel();
			ApiServices.ApiServicesClient client = new ApiServices.ApiServicesClient(channel);

			// Make gRPC request
			AddBlockResponse response = await client.AddBlockAsync(
				new AddBlockRequest
				{
					Blocks = new SP.Api.Models.Blocks
					{
						Id = blocks.Id,
						IpAddress = blocks.IpAddress,
						IpAddress1 = Convert.ToInt32(blocks.IpAddress.Split(".")[0]),
						IpAddress2 = Convert.ToInt32(blocks.IpAddress.Split(".")[1]),
						IpAddress3 = Convert.ToInt32(blocks.IpAddress.Split(".")[2]),
						IpAddress4 = Convert.ToInt32(blocks.IpAddress.Split(".")[3]),
						Hostname = blocks.Hostname,
						Country = blocks.Country ?? "",
						City = blocks.City ?? "",
						ISP = blocks.ISP ?? "",
						Details = blocks.Details,
						Date = Timestamp.FromDateTime(DateTime.SpecifyKind(blocks.EventDate, DateTimeKind.Utc)),
						FirewallRuleName = blocks.FirewallRuleName,
						IsBlocked = blocks.IsBlocked,
						AttackType = (int) blocks.AttackType
					}
				});

			// Diagnostics
			log.Debug($"{nameof(AddBlock)} received server response: {response.Result}");

			// Clean up
			await channel.ShutdownAsync();

			return response.Result;
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> UpdateBlock(Blocks block)
		{
			// Diagnostics
			log.Debug($"{nameof(UpdateBlock)} called for {block.IpAddress}");

			// Get connection
			GrpcChannel channel = GetChannel();
			ApiServices.ApiServicesClient client = new ApiServices.ApiServicesClient(channel);

			// Make gRPC request
			UpdateBlockResponse response = await client.UpdateBlockAsync(
				new UpdateBlockRequest
				{
					Blocks = new SP.Api.Models.Blocks
					{
						Id = block.Id,
						IpAddress = block.IpAddress,
						Hostname = block.Hostname,
						Country = block.Country ?? "",
						City = block.City ?? "",
						ISP = block.ISP ?? "",
						Details = block.Details,
						Date = Timestamp.FromDateTime(DateTime.SpecifyKind(block.EventDate, DateTimeKind.Utc)),
						FirewallRuleName = block.FirewallRuleName,
						IsBlocked = block.IsBlocked,
						AttackType = (int) block.AttackType
					}
				});

			// Diagnostics
			log.Debug($"{nameof(UpdateBlock)} received server response: {response.Result}");

			// Clean up
			await channel.ShutdownAsync();

			return response.Result;
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> StatisticsUpdateBlocks(Blocks block)
		{
			// Diagnostics
			log.Debug($"{nameof(StatisticsUpdateBlocks)} called for {block.IpAddress}");

			// Get connection
			GrpcChannel channel = GetChannel();
			ApiServices.ApiServicesClient client = new ApiServices.ApiServicesClient(channel);

			// Make gRPC request
			StatisticsUpdateBlocksResponse response = await client.StatisticsUpdateBlocksAsync(
				new StatisticsUpdateBlocksRequest
				{
					Blocks = new SP.Api.Models.Blocks
					{
						Id = block.Id,
						IpAddress = block.IpAddress,
						Hostname = block.Hostname,
						Country = block.Country ?? "",
						City = block.City ?? "",
						ISP = block.ISP ?? "",
						Details = block.Details,
						Date = Timestamp.FromDateTime(DateTime.SpecifyKind(block.EventDate, DateTimeKind.Utc)),
						FirewallRuleName = block.FirewallRuleName,
						IsBlocked = block.IsBlocked,
						AttackType = (int) block.AttackType
					}
				});

			// Diagnostics
			log.Debug($"{nameof(StatisticsUpdateBlocks)} received server response: {response.Result}");

			// Clean up
			await channel.ShutdownAsync();

			return response.Result;
		}

		/// <summary>
		/// </summary>
		/// <param name="accessAttempt"></param>
		/// <param name="detectIPRange"></param>
		/// <param name="fromTime"></param>
		/// <returns>
		/// The number of login attempts that took place within the timespan of the current time vs the fromTime. If -1
		/// gets returned, the call failed.
		/// </returns>
		public async Task<int> GetLoginAttempts(SP.Models.AccessAttempts accessAttempt, bool detectIPRange,
			DateTime fromTime)
		{
			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} called for IP {accessAttempt.IpAddress}");

			// Get connection
			GrpcChannel channel = GetChannel();
			ApiServices.ApiServicesClient client = new ApiServices.ApiServicesClient(channel);

			// Make gRPC request
			GetAccessAttemptsResponse response = await client.GetLoginAttemptsAsync(
				new GetAccessAttemptsRequest
				{
					DetectIPRange = detectIPRange,
					FromTime = Timestamp.FromDateTime(DateTime.SpecifyKind(fromTime, DateTimeKind.Utc)),
					AccessAttempt = new AccessAttempt
					{
						Id = accessAttempt.Id,
						IpAddress = accessAttempt.IpAddress,
						IpAddress1 = accessAttempt.IpAddress1,
						IpAddress2 = accessAttempt.IpAddress2,
						IpAddress3 = accessAttempt.IpAddress3,
						IpAddress4 = accessAttempt.IpAddress4,
						IpAddressRange = accessAttempt.IpAddressRange,
						Details = accessAttempt.Details,
						EventDate = Timestamp.FromDateTime(DateTime.SpecifyKind(accessAttempt.EventDate,
							DateTimeKind.Utc)),
						AttackType = (int) accessAttempt.AttackType,
						Custom1 = accessAttempt.Custom1,
						Custom2 = accessAttempt.Custom2,
						Custom3 = accessAttempt.Custom3
					}
				});

			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} received server response: {response.Result}");

			// Clean up
			await channel.ShutdownAsync();

			return response.Result;
		}

		/// <summary>
		/// </summary>
		/// <param name="accessAttempt"></param>
		/// <returns></returns>
		public async Task<bool> AddLoginAttempt(SP.Models.AccessAttempts accessAttempt)
		{
			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} called for IP {accessAttempt.IpAddress}");

			// Get connection
			GrpcChannel channel = GetChannel();
			ApiServices.ApiServicesClient client = new ApiServices.ApiServicesClient(channel);

			// Make gRPC request
			AddAccessAttemptResponse response = await client.AddLoginAttemptAsync(
				new AddAccessAttemptRequest
				{
					AccessAttempts = new AccessAttempt
					{
						Id = accessAttempt.Id,
						IpAddress = accessAttempt.IpAddress,
						IpAddress1 = accessAttempt.IpAddress1,
						IpAddress2 = accessAttempt.IpAddress2,
						IpAddress3 = accessAttempt.IpAddress3,
						IpAddress4 = accessAttempt.IpAddress4,
						IpAddressRange = accessAttempt.IpAddressRange,
						Details = accessAttempt.Details,
						EventDate = Timestamp.FromDateTime(DateTime.SpecifyKind(accessAttempt.EventDate,
							DateTimeKind.Utc)),
						AttackType = (int) accessAttempt.AttackType,
						Custom1 = accessAttempt.Custom1,
						Custom2 = accessAttempt.Custom2,
						Custom3 = accessAttempt.Custom3
					}
				});

			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} received server response: {response.Result}");

			// Clean up
			await channel.ShutdownAsync();

			return response.Result;
		}

		/// <summary>
		/// </summary>
		/// <param name="accessAttempt"></param>
		/// <returns></returns>
		public async Task<bool> StatisticsUpdateBlocks(SP.Models.AccessAttempts accessAttempt)
		{
			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} called for IP {accessAttempt.IpAddress}");

			// Get connection
			GrpcChannel channel = GetChannel();
			ApiServices.ApiServicesClient client = new ApiServices.ApiServicesClient(channel);

			// Make gRPC request
			AddAccessAttemptResponse response = await client.AddLoginAttemptAsync(
				new AddAccessAttemptRequest
				{
					AccessAttempts = new AccessAttempt
					{
						Id = accessAttempt.Id,
						IpAddress = accessAttempt.IpAddress,
						IpAddress1 = accessAttempt.IpAddress1,
						IpAddress2 = accessAttempt.IpAddress2,
						IpAddress3 = accessAttempt.IpAddress3,
						IpAddress4 = accessAttempt.IpAddress4,
						IpAddressRange = accessAttempt.IpAddressRange,
						Details = accessAttempt.Details,
						EventDate = Timestamp.FromDateTime(DateTime.SpecifyKind(accessAttempt.EventDate,
							DateTimeKind.Utc)),
						AttackType = (int) accessAttempt.AttackType,
						Custom1 = accessAttempt.Custom1,
						Custom2 = accessAttempt.Custom2,
						Custom3 = accessAttempt.Custom3
					}
				});

			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} received server response: {response.Result}");

			// Clean up
			await channel.ShutdownAsync();

			return response.Result;
		}
	}
}