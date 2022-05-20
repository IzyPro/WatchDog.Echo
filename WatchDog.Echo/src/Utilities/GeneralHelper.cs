using System;
using System.Collections.Generic;
using System.Text;

namespace WatchDog.Echo.src.Utilities
{
    internal static class GeneralHelper
    {
        public static Tuple<string, string> SplitWebhook(string webhook)
        {
            Uri uriAddress = new Uri(webhook);
            var baseUrl = uriAddress.GetLeftPart(UriPartial.Authority);
            var channelAddress = webhook.Replace(baseUrl, "");
            return new Tuple<string, string>(baseUrl, channelAddress); 
        }
    }
}
