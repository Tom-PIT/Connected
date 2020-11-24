/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.Configuration;
using System.Text;

namespace TomPIT.Connected.Printing.Client.Configuration
{
    internal static class Settings
    {
        public static string CdnUrl { get; private set; }

        public static string Token { get; private set; }

        public static string AvailablePrinters { get; private set; }

        public static void ResetSettings()
        {
            CdnUrl = ConfigurationManager.AppSettings["cdnUrl"];
            AvailablePrinters = ConfigurationManager.AppSettings["availablePrinters"];

            string tokenString = ConfigurationManager.AppSettings["token"];
            Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenString));

            string tmpString = ConfigurationManager.AppSettings["loggingLevel"];
            Logging.Level = Enum.TryParse<LoggingLevel>(tmpString, true, out LoggingLevel result) ? result : LoggingLevel.Error;

            tmpString = ConfigurationManager.AppSettings["exceptionLoggingLevel"];
            Logging.ExceptionLogging = Enum.TryParse<ExceptionLoggingLevel>(tmpString, true, out ExceptionLoggingLevel resultExcLogging) ? resultExcLogging : ExceptionLoggingLevel.ErrorMessage;

        }

        static Settings()
        {
            ResetSettings();
        }

    }
}
