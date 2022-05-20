using System;
using System.Collections.Generic;
using System.Text;
using WatchDog.Echo.src.Utilities;

namespace WatchDog.Echo.src.Models
{
    public class EchoSettings
    {
        public long EchoIntervalInMinutes { get; set; } = Constants.DefaultEchoIntervalinMinutes;
        public long FailedEchoAlertIntervalInMinutes { get; set; } = Constants.FailedEchoAlertIntervalInMinutes;
        public string? HostURLs { get; set; }
        public string? WebhookURLs { get; set; }
        public string? EmailAddress { get; set; }
    }

    internal class EchoInterval
    {
        public static long EchoIntervalInMinutes { get; set; }
        public static long FailedEchoAlertIntervalInMinutes { get; set; }

    }
    internal class MicroService
    {
        public static string? MicroServicesURL { get; set; }
    }

    internal class WebHooks
    {
        public static string? WebhookURLs { get; set; }  
    }
    internal class MailAlerts
    {
        public static string? ToEmailAddress { get; set; }
    }
}
