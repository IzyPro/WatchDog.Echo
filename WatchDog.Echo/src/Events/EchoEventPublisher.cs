using System;
using System.Collections.Generic;
using System.Text;

namespace WatchDog.Echo.src.Events
{
    public class EchoEventPublisher
    {

        private static readonly EchoEventPublisher _instance = new EchoEventPublisher();    
        static EchoEventPublisher()
        {

        }

        private EchoEventPublisher()
        {

        }
        public static EchoEventPublisher Instance { get { return _instance; } }



        public void PublishEchoFailedEvent(string fromHost, string toHost)
        {
            EchoEventsArgs args = new EchoEventsArgs();
            args.FromHost = fromHost;
            args.ToHost = toHost;
            OnEchoFailed(args);
        }

        protected virtual void OnEchoFailed(EchoEventsArgs e)
        {
            EventHandler<EchoEventsArgs> handler = OnEchoFailedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<EchoEventsArgs> OnEchoFailedEvent;
    }
}
