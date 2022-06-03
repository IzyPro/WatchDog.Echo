using System;

namespace WatchDog.Echo.src.Events
{
    public class EchoEventsArgs : EventArgs   
    {
        public string FromHost { get; set; }
        public string ToHost { get; set; }
    }
}
