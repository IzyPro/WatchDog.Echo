using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WatchDog.Echo.src.Utilities;

namespace WatchDog.Echo.src.Services
{
    internal class EchoRESTService
    {
        private readonly HttpClient _client;

        public EchoRESTService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> SendRESTEcho(string url)
        {
            _client.BaseAddress = new Uri(url);
            return await _client.GetAsync(Constants.RestEndpoint);
        }
    }
}
