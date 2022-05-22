using System;
using System.Collections.Generic;
using System.Text;

namespace WatchDog.Echo.src.Exceptions
{
    internal class WatchDogEchoMailSettingsException : Exception
    {
        internal WatchDogEchoMailSettingsException()
        {

        }

        internal WatchDogEchoMailSettingsException(string message) : base(String.Format("WatchDog MailSettings Exception: {0}", message))
        {

        }
    }
}
