using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WatchDog.Echo.src.Models;

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
            //var resultContent = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Task failed.");
            }
        }


        public async Task SendEmailNotificationAsync(string content, string[] toEmail, MailSettings mailSettings)
        {
            MimeMessage message = new MimeMessage();

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
            bodyBuilder.HtmlBody = content;
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                //client.Connect("in-v3.mailjet.com", 465, true);
                client.Connect(mailSettings.MailHost, (int)mailSettings.MailPort, true);
                client.Authenticate(mailSettings.MailPubKey, mailSettings.MailSecKey);
                await client.SendAsync(message);
                client.Disconnect(true);
            }

        }
    }
}
