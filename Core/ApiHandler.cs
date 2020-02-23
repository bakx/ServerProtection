using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SP.Core.Interfaces;
using SP.Models;

namespace SP.Core
{
    public class ApiHandler : IApiHandler
    {
        // Configuration settings
        private readonly HttpClient httpClient;

        // Diagnostics
        private readonly ILogger<ApiHandler> log;

        /// <summary>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="httpClient"></param>
        public ApiHandler(ILogger<ApiHandler> log, HttpClient httpClient)
        {
            this.log = log;
            this.httpClient = httpClient;
        }

        /// <summary>
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public async Task<List<Blocks>> GetUnblock(int minutes)
        {
            // Contact the api
            string path = $"/block/GetUnblocks?minutes={minutes}";

            HttpResponseMessage message = await httpClient.GetAsync(path);

            if (message.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<List<Blocks>>(await message.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
            }

            log.LogError(
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
            HttpResponseMessage message = await httpClient.PostAsync(path, content);

            // Diagnostics 
            if (!message.IsSuccessStatusCode)
            {
                log.LogError(
                    $"Invalid response code while calling {path}. Status code: {message.StatusCode}, {message.RequestMessage}");
            }

            return message;
        }
    }
}