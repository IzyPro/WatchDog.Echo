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

        public async Task SendWebhookNotificationAsync(string message, string webhookUrl)
        {
            string contentObjectJson;
            if (webhookUrl.ToLower().Contains("discord"))
            {
                var contentObject = new { content = message, username = "", avatar_url = "", tts = false };
                contentObjectJson = JsonSerializer.Serialize(contentObject);
            }
            else
            {
                if (webhookUrl.ToLower().Contains("office.com"))
                    message = $"<pre>{message}</pre>";
                var contentObject = new { text = message };
                contentObjectJson = JsonSerializer.Serialize(contentObject);
            }
            var content = new StringContent(contentObjectJson, Encoding.UTF8, "application/json");


            var result = await _client.PostAsync(webhookUrl, content);
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Task failed.");
            }
        }


        public async Task SendEmailNotificationAsync(string url, string content, string[] toEmail, MailSettings mailSettings)
        {
            MimeMessage message = new MimeMessage();
            var body = $"<img src='https://i.ibb.co/thnngfd/Echo-Signature.png' alt='WatchDog Echo Logo' style='width: 100%;'><pre>\nAlert!\n{url} failed to respond to echo from {MicroService.MicroServiceClientHost}.\n\n<b>Response:</b> {content}\n<b>Happened At:</b> {DateTime.Now.ToString("dd /MM/yyyy hh:mm tt")}\n\nSincerely,\nYour WatchDog🐶</pre>";

            MailboxAddress from = new MailboxAddress("WatchDog Echo", mailSettings.MailFrom);
            List<MailboxAddress> to = new List<MailboxAddress>();
            foreach (var email in toEmail)
            {
                to.Add(new MailboxAddress("WatchDog Echo Client", email));
            }

            message.From.Add(from);
            message.To.AddRange(to);
            message.Subject = "WatchDog Echo Service Alert";

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
            var description = $"{url} failed to respond to an echo from {MicroService.MicroServiceClientHost}.";
            var contentObject = new { Description = description, Response = message, Server = url, HappenedAt = happenedAt };
            var contentObjectJson = JsonSerializer.Serialize(contentObject);
            var content = new StringContent(contentObjectJson, Encoding.UTF8, "application/json");

            await _client.PostAsync(WebHooks.CustomAlertWebhookURL, content);
        }
    }
}
