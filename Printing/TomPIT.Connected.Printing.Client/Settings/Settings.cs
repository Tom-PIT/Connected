/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TomPIT.Connected.Printing.Client.Configuration
{
    internal static class Settings
    {
        public static string CdnUrl { get; private set; }

        public static string Token { get; private set; }

        public static string AvailablePrinters { get; private set; }

        public static List<string> PrinterList { get; private set; } = new List<string>();

        public static Dictionary<string, string> PrinterNameMappings { get; private set; } = new Dictionary<string, string>();

        public static void ResetSettings()
        {
            PrinterNameMappings.Clear();
            PrinterList.Clear();

            var printerList = ConfigurationManager.GetSection("printers") as List<string>;
            var printerMappings = ConfigurationManager.GetSection("printerMappings") as Dictionary<string, string>;

            CdnUrl = ConfigurationManager.AppSettings["cdnUrl"];

            AvailablePrinters = ConfigurationManager.AppSettings["availablePrinters"];
            if (printerList != null)
            {
                PrinterList.AddRange(printerList);
            }

            //printer name mappings - first, read from string and add to dictionary
            var printerNameMappings = ConfigurationManager.AppSettings["printerNameMappings"];
            if (!string.IsNullOrWhiteSpace(printerNameMappings))
            {
                PrinterNameMappings = printerNameMappings.Split(';')
                    .Select(value => value.Split('='))
                    .ToDictionary(keyValuePair => keyValuePair[0], keyValuePair => keyValuePair[1]);
            }
            //then from list
            if (printerMappings != null)
            {
                foreach (var mapping in printerMappings)
                {
                    if (PrinterNameMappings.ContainsKey(mapping.Key))
                        continue;

                    PrinterNameMappings.Add(mapping.Key, mapping.Value);
                }
            }

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
