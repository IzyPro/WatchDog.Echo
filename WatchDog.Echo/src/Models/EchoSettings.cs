﻿using WatchDog.Echo.src.Enums;
using WatchDog.Echo.src.Utilities;

namespace WatchDog.Echo.src.Models
{
    public class EchoSettings
    {
        public long EchoIntervalInMinutes { get; set; } = Constants.DefaultEchoIntervalinMinutes;
        public long FailedEchoAlertIntervalInMinutes { get; set; } = Constants.FailedEchoAlertIntervalInMinutes;
        public string ClientHost { get; set; }
        public string? EchoTargetURLs { get; set; }
        public string? WebhookURLs { get; set; }
        public string? CustomAlertWebhookURL { get; set; }
        public string? EmailAddresses { get; set; }
        public MailSettings MailConfig { get; set; }
        public ProtocolEnum Protocol { get; set; } = ProtocolEnum.gRPC;
    }


    public class MailSettings
    {
        public string? MailHost { get; set; }
        public int? MailPort { get; set; }
        public string? MailPubKey { get; set; }
        public string? MailSecKey { get; set; }
        public string? MailFrom { get; set; }
    }

    internal class EchoInterval
    {
        public static long EchoIntervalInMinutes { get; set; }
        public static long FailedEchoAlertIntervalInMinutes { get; set; }

    }
    internal class MicroService
    {
        public static string? MicroServicesURL { get; set; }
        public static string? MicroServiceClientHost { get; set; }  
    }

    internal class WebHooks
    {
        public static string? WebhookURLs { get; set; }
        public static string? CustomAlertWebhookURL { get; set; }
    }
    internal class MailAlerts
    {
        public static string? ToEmailAddress { get; set; }
    }

    internal class MailConfiguration
    {
        public static MailSettings? MailConfigurations { get; set; }
    }
    internal class Protocol
    {
        public static ProtocolEnum? ProtocolType { get; set; }
    }
}
