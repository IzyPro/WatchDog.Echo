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

        public NotificationServices()
        {
            _client = new HttpClient();
        }

        public async Task SendWebhookNotificationAsync(string message, string webhookBaseUrl, string webhookEndpoint)
        {
            _client.BaseAddress = new Uri(webhookBaseUrl);
            var contentObject = new { text = message };
            var contentObjectJson = JsonSerializer.Serialize(contentObject);
            var content = new StringContent(contentObjectJson, Encoding.UTF8, "application/json");

            var result = await _client.PostAsync(webhookEndpoint, content);
            var resultContent = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Task failed.");
            }
        }
    }
}
