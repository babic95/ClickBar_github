using Easy.Logger;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Logging
{
    public class Logger
    {
        #region Fields
        private static ILog _log = LogManager.GetLogger(Assembly.GetEntryAssembly(), "CCS_ClickBar");
        #endregion Fields

        #region Methods
        public static void ConfigureLog(string logFolder, bool porudzbine = false)
        {
            try
            {
                string nameFile = string.Format($"CCS_ClickBar-{DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss")}{(porudzbine ? "_porudzbine" : "")}.log");
                var LogFilePattern = new PatternString(Path.Combine(logFolder, nameFile));

                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                if (!_log.Logger.Repository.Configured)
                {
                    // Setup RollingFileAppender
                    var fileAppender = new RollingFileAppender
                    {
                        Layout = new PatternLayout("%d [%t]%-5p %c - %m ex: %exception%n"),
                        MaximumFileSize = "10MB",
                        MaxSizeRollBackups = 100,
                        RollingStyle = RollingFileAppender.RollingMode.Size,
                        AppendToFile = true,
                        File = LogFilePattern.ConversionPattern,
                        ImmediateFlush = true,
                        LockingModel = new FileAppender.MinimalLock(),
                        Name = "XXXRollingFileAppender"
                    };

                    fileAppender.ActivateOptions();

                    var asyncForwardingAppender = new AsyncBufferingForwardingAppender();
                    asyncForwardingAppender.AddAppender(fileAppender);
                    asyncForwardingAppender.ActivateOptions();

                    BasicConfigurator.Configure(logRepository, asyncForwardingAppender);
                }

                Log.LogDebug += Log_LogDebug;
                Log.LogError += Log_LogError;

                Log.Debug("--------------------------------===========================CCS ClickBar IS RUNNING===========================--------------------------------");

                try
                {
                    //Log.Debug(string.Format("Application version : {0}", Assembly.GetExecutingAssembly().GetName().Version));
                }
                catch (Exception ex)
                {
                    Log.Error("Logger - ConfigureLog, Unable to extract application version!", ex);
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Logger - ConfigureLog - ", ex));
            }
        }
        private static void Log_LogDebug(LogEventArgs e)
        {
            if (e.Exception == null)
            {
                _log.Debug(e.Message);
            }
            else
            {
                _log.Debug(e.Message, e.Exception);
            }
        }

        private static void Log_LogError(LogEventArgs e)
        {
            if (e.Exception == null)
            {
                _log.Error(e.Message);
            }
            else
            {
                _log.Error(e.Message, e.Exception);
            }
        }
        #endregion Methods
    }
}
