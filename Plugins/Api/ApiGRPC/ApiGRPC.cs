using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Api.Models;
using SP.Plugins;
using Blocks = SP.Models.Blocks;

namespace Plugins
{
	public class ApiGrpc : IPluginBase, IApiHandler
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
		public Task<bool> Initialize(PluginOptions options)
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
		public Task<bool> Configure()
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
		/// <returns></returns>
		private ApiServices.ApiServicesClient GetClient()
		{
			return new ApiServices.ApiServicesClient(GetChannel());
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		/// <param name="loginAttemptHandler"></param>
		/// <returns></returns>
		public async Task<bool> RegisterLoginAttemptHandler(IPluginBase.LoginAttempt loginAttemptHandler)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		/// <param name="blockHandler"></param>
		/// <returns></returns>
		public async Task<bool> RegisterBlockHandler(IPluginBase.Block blockHandler)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> RegisterUnblockHandler(IPluginBase.Unblock unblockHandler)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> LoginAttemptEvent(SP.Models.LoginAttempts loginAttempt)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> BlockEvent(Blocks block)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> UnblockEvent(Blocks block)
		{
			return await Task.FromResult(true);
		}

		public async Task<List<Blocks>> GetUnblock(int minutes)
		{
			// Diagnostics
			log.Debug($"{nameof(GetUnblock)} called with param {minutes}");

			// Make gRPC request
			GetUnblocksResponse response = await GetClient().GetUnblocksAsync(
				new GetUnblocksRequest
				{
					Minutes = minutes
				});

			// Diagnostics
			log.Debug($"{nameof(GetUnblock)} received server response: {response.Blocks.Count} items");

			// Convert models.
			return response.Blocks.Select(blocks => new Blocks
				{
					Id = blocks.Id,
					IpAddress = blocks.IpAddress,
					Hostname = blocks.Hostname,
					Country = blocks.Country,
					City = blocks.City,
					ISP = blocks.ISP,
					Details = blocks.Details,
					Date = blocks.Date.ToDateTime(),
					FirewallRuleName = blocks.FirewallRuleName,
					IsBlocked = blocks.IsBlocked[0]
				})
				.ToList();
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> AddBlock(Blocks block)
		{
			// Diagnostics
			log.Debug($"{nameof(AddBlock)} called for {block.IpAddress}");

			// Make gRPC request
			AddBlockResponse response = await GetClient().AddBlockAsync(
				new AddBlockRequest
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
						Date = Timestamp.FromDateTime(DateTime.SpecifyKind(block.Date, DateTimeKind.Utc)),
						FirewallRuleName = block.FirewallRuleName,
						IsBlocked = ByteString.CopyFrom(block.IsBlocked == 0 ? "0" : "1", Encoding.Unicode)
					}
				});

			// Diagnostics
			log.Debug($"{nameof(AddBlock)} received server response: {response.Result}");

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

			// Make gRPC request
			UpdateBlockResponse response = await GetClient().UpdateBlockAsync(
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
						Date = Timestamp.FromDateTime(DateTime.SpecifyKind(block.Date, DateTimeKind.Utc)),
						FirewallRuleName = block.FirewallRuleName,
						IsBlocked = ByteString.CopyFrom(block.IsBlocked == 0 ? "0" : "1", Encoding.Unicode)
					}
				});

			// Diagnostics
			log.Debug($"{nameof(UpdateBlock)} received server response: {response.Result}");

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

			// Make gRPC request
			StatisticsUpdateBlocksResponse response = await GetClient().StatisticsUpdateBlocksAsync(
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
						Date = Timestamp.FromDateTime(DateTime.SpecifyKind(block.Date, DateTimeKind.Utc)),
						FirewallRuleName = block.FirewallRuleName,
						IsBlocked = ByteString.CopyFrom(block.IsBlocked == 0 ? "0" : "1", Encoding.Unicode)
					}
				});

			// Diagnostics
			log.Debug($"{nameof(StatisticsUpdateBlocks)} received server response: {response.Result}");

			return response.Result;
		}

		/// <summary>
		/// </summary>
		/// <param name="loginAttempt"></param>
		/// <param name="detectIPRange"></param>
		/// <param name="fromTime"></param>
		/// <returns>
		/// The number of login attempts that took place within the timespan of the current time vs the fromTime. If -1
		/// gets returned, the call failed.
		/// </returns>
		public async Task<int> GetLoginAttempts(SP.Models.LoginAttempts loginAttempt, bool detectIPRange, DateTime fromTime)
		{
			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} called for IP {loginAttempt.IpAddress}");

			// Make gRPC request
			GetLoginAttemptsResponse response = await GetClient().GetLoginAttemptsAsync(
				new GetLoginAttemptsRequest
				{
					DetectIPRange = detectIPRange,
					FromTime = Timestamp.FromDateTime(DateTime.SpecifyKind(fromTime, DateTimeKind.Utc)),
					LoginAttempts = new LoginAttempts
					{
						Id = loginAttempt.Id,
						IpAddress = loginAttempt.IpAddress,
						IpAddress1 = Convert.ToString(loginAttempt.IpAddress1),
						IpAddress2 = Convert.ToString(loginAttempt.IpAddress2),
						IpAddress3 = Convert.ToString(loginAttempt.IpAddress3),
						IpAddress4 = Convert.ToString(loginAttempt.IpAddress4),
						IpAddressRange = loginAttempt.IpAddressRange,
						Details = loginAttempt.Details,
						EventDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loginAttempt.EventDate, DateTimeKind.Utc))
					}
				});

			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} received server response: {response.Result}");

			return response.Result;
		}

		/// <summary>
		/// </summary>
		/// <param name="loginAttempt"></param>
		/// <returns></returns>
		public async Task<bool> AddLoginAttempt(SP.Models.LoginAttempts loginAttempt)
		{
			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} called for IP {loginAttempt.IpAddress}");

			// Make gRPC request
			AddLoginAttemptResponse response = await GetClient().AddLoginAttemptAsync(
				new AddLoginAttemptRequest
				{
					LoginAttempts = new LoginAttempts
					{
						Id = loginAttempt.Id,
						IpAddress = loginAttempt.IpAddress,
						IpAddress1 = Convert.ToString(loginAttempt.IpAddress1),
						IpAddress2 = Convert.ToString(loginAttempt.IpAddress2),
						IpAddress3 = Convert.ToString(loginAttempt.IpAddress3),
						IpAddress4 = Convert.ToString(loginAttempt.IpAddress4),
						IpAddressRange = loginAttempt.IpAddressRange,
						Details = loginAttempt.Details,
						EventDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loginAttempt.EventDate, DateTimeKind.Utc))
					}
				});

			// Diagnostics
			log.Debug($"{nameof(GetLoginAttempts)} received server response: {response.Result}");

			return response.Result;
		}
	}
}