using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SP.Core.Tools
{
    public static class IPData
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task<DataModel> GetDetails(string url, string apiKey, string ip)
        {
            string requestUrl = string.Format(url, ip, apiKey);
            string json = await HttpClient.GetStringAsync(requestUrl);

            JObject o = JObject.Parse(json);

            DataModel model = new DataModel
            {
                City = o["city"].ToString(),
                Country = o["country_name"].ToString(),
                ISP = o["asn"]["name"].ToString()
            };

            return model;
        }
    }

    public class DataModel
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string ISP { get; set; }
    }
}