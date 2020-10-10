using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Plugins;

namespace Plugins
{
	public class ApiTcp : IPluginBase, IApiHandler
	{
		private static readonly HttpClient HttpClient = new HttpClient();
		private string apiUrl;
		private string apiToken;

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
					.ForContext(typeof(ApiTcp));

				// Assign config variables
				apiUrl = config["Url"];
				apiToken = config["Token"];

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
		public async Task<bool> Configure()
		{
			try
			{
				// Set up http(s) client
				HttpClient.BaseAddress = new Uri(apiUrl);
				HttpClient.DefaultRequestHeaders.Accept.Clear();
				HttpClient.DefaultRequestHeaders.Add("Key", apiToken);
				HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				return await Task.FromResult(true);
			}
			catch (Exception e)
			{
				log.Error("{0}", e);
				return await Task.FromResult(false);
			}
			finally
			{
				if (log == null)
				{
					Console.WriteLine("Completed Configuration stage");
				}
				else
				{
					log.Information("Completed Configuration stage");
				}
			}
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
		public async Task<bool> LoginAttemptEvent(LoginAttempts loginAttempt)
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
			// Contact the api
			string path = $"/block/GetUnblocks?minutes={minutes}";

			HttpResponseMessage message = await HttpClient.GetAsync(path);

			if (message.IsSuccessStatusCode)
			{
				return JsonSerializer.Deserialize<List<Blocks>>(await message.Content.ReadAsStringAsync(),
					new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase
					});
			}

			log.Error(
				$"Invalid response code while calling {path}. Status code: {message.StatusCode}, {message.RequestMessage}");
			return null;
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> AddBlock(Blocks block)
		{
			// Contact the api
			const string path = "/block/AddBlock";

			// Add content
			HttpContent content = new StringContent(JsonSerializer.Serialize(block), Encoding.UTF8, "application/json");

			// Execute the request
			HttpResponseMessage message = await PostRequest(path, content);
			return message.IsSuccessStatusCode;
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> UpdateBlock(Blocks block)
		{
			// Contact the api
			const string path = "/block/UpdateBlock";

			// Add content
			HttpContent content = new StringContent(JsonSerializer.Serialize(block), Encoding.UTF8, "application/json");

			// Execute the request
			HttpResponseMessage message = await PostRequest(path, content);
			return message.IsSuccessStatusCode;
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> StatisticsUpdateBlocks(Blocks block)
		{
			// Contact the api
			const string path = "/statistics/UpdateBlock";

			// Add content
			HttpContent content = new StringContent(JsonSerializer.Serialize(block), Encoding.UTF8, "application/json");

			// Execute the request
			HttpResponseMessage message = await PostRequest(path, content);
			return message.IsSuccessStatusCode;
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
		public async Task<int> GetLoginAttempts(LoginAttempts loginAttempt, bool detectIPRange, DateTime fromTime)
		{
			// Contact the api
			string path = $"/loginAttempts/GetLoginAttempts?detectIPRange={detectIPRange}&fromTime={fromTime}";

			// Add content
			HttpContent content =
				new StringContent(JsonSerializer.Serialize(loginAttempt), Encoding.UTF8, "application/json");

			// Execute the request
			HttpResponseMessage message = await PostRequest(path, content);

			return message.IsSuccessStatusCode ? Convert.ToInt32(message.Content.ReadAsStringAsync().Result) : -1;
		}

		/// <summary>
		/// </summary>
		/// <param name="loginAttempt"></param>
		/// <returns></returns>
		public async Task<bool> AddLoginAttempt(LoginAttempts loginAttempt)
		{
			// Contact the api
			const string path = "/loginAttempts/Add";

			// Add content
			HttpContent content =
				new StringContent(JsonSerializer.Serialize(loginAttempt), Encoding.UTF8, "application/json");

			// Execute the request
			HttpResponseMessage message = await PostRequest(path, content);
			return message.IsSuccessStatusCode;
		}

		/// <summary>
		/// </summary>
		/// <param name="path"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		private async Task<HttpResponseMessage> PostRequest(string path, HttpContent content)
		{
			// Post message
			HttpResponseMessage message = await HttpClient.PostAsync(path, content);

			// Diagnostics 
			if (!message.IsSuccessStatusCode)
			{
				log.Error(
					$"Invalid response code while calling {path}. Status code: {message.StatusCode}, {message.RequestMessage}");
			}

			return message;
		}
	}
}