using System;
using System.Collections.Generic;
using System.Text;

namespace WatchDog.Echo.src.Events
{
    class EchoEventPublisher
    {
        public void PublishEvent(string fromHost, string toHost)
        {
            EchoEventsArgs args = new EchoEventsArgs();
            args.FromHost = fromHost;
            args.ToHost = toHost;
            OnEchoFailed(args);

        }

        protected virtual void OnEchoFailed (EchoEventsArgs e)
        {
            EventHandler<EchoEventsArgs> handler = EchoFailedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<EchoEventsArgs> EchoFailedEvent;
    }

    public class EchoEventSubscriber
    {
        private EchoEventPublisher _publisher;
        
        public void Subscribe(EventHandler<EchoEventsArgs> onEchoFailed)
        {
            _publisher.EchoFailedEvent += onEchoFailed;
        }


    }
}
