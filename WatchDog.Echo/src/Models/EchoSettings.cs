﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WatchDog.Echo.src.Models
{
    public class EchoSettings
    {
        public long EchoIntervalInMinutes { get; set; }
        public string? HostURLs { get; set; }
        public string? SlackChannelAddress { get; set; }
    }

    internal class EchoInterval
    {
        public static long EchoIntervalInMinutes { get; set; }
    }
    internal class MicroService
    {
        public static string? MicroServicesURL { get; set; }
    }

    internal class WebHooks
    {
        public static string? SlackChannelHook { get; set; }  
    }
}
