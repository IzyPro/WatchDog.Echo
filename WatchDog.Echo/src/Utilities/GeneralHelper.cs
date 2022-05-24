using System;
using System.Reflection;

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

        public static Tuple<bool, string> IsAnyNullOrEmpty(this object myObject)
        {
            
            foreach (PropertyInfo pi in myObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(myObject);
                    if (string.IsNullOrEmpty(value))
                    {
                        return new Tuple<bool, string>(true, (string)pi.Name);
                    }
                }
                else
                {
                    if(pi.GetValue(myObject) == null)
                    {
                        return new Tuple<bool, string>(true, (string)pi.Name);
                    }
                }
            }

            return new Tuple<bool, string>(false, "");
        }
    }
}
