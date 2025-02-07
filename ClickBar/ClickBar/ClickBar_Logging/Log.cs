using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Logging
{
    public delegate void LogEventHandler(LogEventArgs e);

    public class LogEventArgs : EventArgs
    {
        #region Properties
        public object Message { get; set; }
        public Exception Exception { get; set; }
        #endregion Properties
    }

    public class Log
    {
        #region Fields
        public static event LogEventHandler LogDebug;
        public static event LogEventHandler LogError;
        #endregion Fields

        #region Methods
        public static void Debug(object message, Exception exception)
        {
            LogDebug?.Invoke(new LogEventArgs() { Exception = exception, Message = message });
        }

        public static void Debug(object message)
        {
            Debug(message, null);
        }

        public static void Error(object message, Exception exception)
        {
            LogError?.Invoke(new LogEventArgs() { Exception = exception, Message = message });
        }

        public static void Error(object message)
        {
            Error(message, null);
        }
        #endregion Methods
    }
}