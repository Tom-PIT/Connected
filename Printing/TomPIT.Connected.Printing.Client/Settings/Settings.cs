/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace TomPIT.Connected.Printing.Client.Configuration
{
    internal static class Settings
    {
        public static string CdnUrl { get; private set; }

        public static string Token { get; private set; }

        public static Dictionary<string, string> PrinterNameMappings { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Print job timeout in seconds
        /// </summary>
        public static int PrintJobTimeout { get; private set; } = 30;

        public static void ResetSettings()
        {
            PrinterNameMappings.Clear();

            var printerList = ConfigurationManager.GetSection("printers") as Dictionary<string, string>;

            CdnUrl = ConfigurationManager.AppSettings["cdnUrl"];

            //then from list
            if (printerList != null)
            {
                foreach (var mapping in printerList)
                {
                    if (PrinterNameMappings.ContainsKey(mapping.Key))
                        continue;

                    PrinterNameMappings.Add(mapping.Key, mapping.Value);
                }
            }

            string tokenString = ConfigurationManager.AppSettings["token"];
            Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenString));

            string loggingLevelString = ConfigurationManager.AppSettings["loggingLevel"];
            Logging.Level = Enum.TryParse(loggingLevelString, true, out LoggingLevel result) ? result : LoggingLevel.Error;

            loggingLevelString = ConfigurationManager.AppSettings["exceptionLoggingLevel"];
            Logging.ExceptionLogging = Enum.TryParse(loggingLevelString, true, out ExceptionLoggingLevel resultExcLogging) ? resultExcLogging : ExceptionLoggingLevel.ErrorMessage;

            var timeoutString = ConfigurationManager.AppSettings["printJobTimeout"];
            PrintJobTimeout = int.TryParse(timeoutString, out var tmp) ? tmp : PrintJobTimeout;
        }

        static Settings()
        {
            ResetSettings();
        }

    }
}
