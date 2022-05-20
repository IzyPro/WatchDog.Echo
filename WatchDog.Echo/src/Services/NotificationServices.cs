using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WatchDog.Echo.src.Services
{
    internal class NotificationServices
    {
        private readonly HttpClient _client;

        private readonly string _slackBaseUrl;
        private readonly string _slackChannelEndpoint;
        public NotificationServices(string slackBaseUrl, string slackChannelEndpoint)
        {
            _client = new HttpClient();
            _slackBaseUrl = slackBaseUrl;   
            _slackChannelEndpoint = slackChannelEndpoint;
        }

        public async Task SendSlackNotificationAsync(string message)
        {
            _client.BaseAddress = new Uri(_slackBaseUrl);
            var contentObject = new { text = message };
            var contentObjectJson = JsonSerializer.Serialize(contentObject);
            var content = new StringContent(contentObjectJson, Encoding.UTF8, "application/json");

            var result = await _client.PostAsync(_slackChannelEndpoint, content);
            var resultContent = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Task failed.");
            }

        }
    }
}
