using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SP.API.Service
{
	internal class ApiService : ApiServices.ApiServicesBase
	{
		// Configuration object
		private readonly IConfigurationRoot config;

		private readonly ILogger log;

		/// <summary>
		/// </summary>
		/// <param name="log"></param>
		/// <param name="config"></param>
		public ApiService(ILogger<ApiService> log, IConfigurationRoot config)
		{
			this.log = log;
			this.config = config;
		}

		public override Task<GetLoginAttemptsResponse> GetLoginAttempts(GetLoginAttemptsRequest request,
			ServerCallContext context)
		{
			return Task.FromResult(new GetLoginAttemptsResponse
			{
				Result = 10
			});
		}

		public override Task<AddLoginAttemptResponse> AddLoginAttempt(AddLoginAttemptRequest request,
			ServerCallContext context)
		{
			return Task.FromResult(new AddLoginAttemptResponse
			{
				Result = true
			});
		}

	}
}