using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WatchDog.Echo.src.Models;
using WatchDog.Echo.src.Utilities;

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
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Task failed.");
            }
        }


        public async Task SendEmailNotificationAsync(string url, string content, string[] toEmail, MailSettings mailSettings)
        {
            MimeMessage message = new MimeMessage();
            var body = $"ALERT!!!\n{url} failed to respond to echo from {MicroService.MicroServiceClientHost}.\nResponse: {content}\nHappened At: {DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")}";

            MailboxAddress from = new MailboxAddress("WatchDog Echo", mailSettings.MailFrom);
            List<MailboxAddress> to = new List<MailboxAddress>();
            foreach(var email in toEmail)
            {
                to.Add(new MailboxAddress("WatchDog Echo Client", email));
            }

            message.From.Add(from);
            message.To.AddRange(to);
            message.Subject = "WatchDog Echo Service Notification";

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(mailSettings.MailHost, (int)mailSettings.MailPort, true);
                client.Authenticate(mailSettings.MailPubKey, mailSettings.MailSecKey);
                await client.SendAsync(message);
                client.Disconnect(true);
            }

        }


        public async Task SendCustomAlertWebhookNotificationAsync(string message, string url, DateTime happenedAt)
        {
            var (_webhookBaseUrl, _webhookEndpoint) = GeneralHelper.SplitWebhook(WebHooks.CustomAlertWebhookURL);
            _client.BaseAddress = new Uri(_webhookBaseUrl);
            var description = $"{url} failed to respond to an echo from {MicroService.MicroServiceClientHost}.";
            var contentObject = new { Description = description, Response = message, Server = url, HappenedAt = happenedAt };
            var contentObjectJson = JsonSerializer.Serialize(contentObject);
            var content = new StringContent(contentObjectJson, Encoding.UTF8, "application/json");

            await _client.PostAsync(_webhookEndpoint, content);
        }
    }
}
