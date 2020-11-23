using System;
using System.Configuration;

namespace TomPIT.Connected.Printing.Client.Configuration
{
    internal static class Settings
    {
        public static string CdnUrl { get; private set; }

        public static Guid Token { get; private set; }


        static Settings()
        {
            CdnUrl = ConfigurationManager.AppSettings["cdnUrl"];

            string tmpString = ConfigurationManager.AppSettings["loggingLevel"];
            Logging.Level = Enum.TryParse<LoggingLevel>(tmpString, true, out LoggingLevel result) ? result : LoggingLevel.Error;

            tmpString = ConfigurationManager.AppSettings["exceptionLoggingLevel"];
            Logging.ExceptionLogging = Enum.TryParse<ExceptionLoggingLevel>(tmpString, true, out ExceptionLoggingLevel resultExcLogging) ? resultExcLogging : ExceptionLoggingLevel.ErrorMessage;
        }

    }
}
