/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
using System.Text;
using TomPIT.Connected.Printing.Client.Configuration;

namespace TomPIT.Connected.Printing.Client.Handlers
{
    internal class PrinterHandler
    {
        private Dictionary<string, string> _printerMapping;

        public PrinterHandler()
        {
            _printerMapping = new Dictionary<string, string>();
        }

        private void SetMappings(List<string> printers, Dictionary<string, string> mappings)
        {
            //add mappings to dictionary
            if (mappings != null)
            {
                foreach (var mapping in mappings)
                {
                    if (_printerMapping.ContainsKey(mapping.Key))
                        continue;
                    _printerMapping[mapping.Key] = mapping.Value;
                }
            }
            //add the rest of printers to mapping
            foreach (var printer in printers)
            {
                if (_printerMapping.ContainsValue(printer))
                    continue;
                _printerMapping[printer] = printer;
            }
        }

        private string PrinterQueueStatus(PrintQueue printQueue)
        {
            var sb = new StringBuilder();

            if (printQueue.IsOffline) sb.Append("Offline;");
            if (printQueue.IsInError) sb.Append("In Error;");
            if (printQueue.IsBusy) sb.Append("Busy;");
            if (printQueue.IsDoorOpened) sb.Append("Door Opened;");
            if (printQueue.IsOutOfMemory) sb.Append("Out of Memory;");
            if (printQueue.IsOutOfPaper) sb.Append("Out of Paper;");
            if (printQueue.IsOutputBinFull) sb.Append("Output Bin Full;");
            if (printQueue.IsPaused) sb.Append("Paused;");
            if (printQueue.IsPowerSaveOn) sb.Append("Power Save On;");
            if (printQueue.IsPaperJammed) sb.Append("Paper Jammed;");
            if (printQueue.IsTonerLow) sb.Append("Toner Low;");
            if (printQueue.IsHidden) sb.Append("Hidden;");
            if (printQueue.IsNotAvailable) sb.Append("Not Available;");
            if (printQueue.IsManualFeedRequired) sb.Append("Manual Feed Required;");
            sb.Append($"Queue Status: {printQueue.QueueStatus};");

            return sb.ToString();
        }

        public bool IsPrinterOnline(string printerPath, out string printerQueueStatus)
        {
            var printerServerName = string.Empty;
            var printerName = printerPath;

            PrintServer printServer = new PrintServer();

            if (printerPath.StartsWith(@"\\"))
            {
                int lastSlashPos = printerPath.LastIndexOf(@"\");

                printerServerName = printerPath.Substring(0, lastSlashPos);
                printerName = printerPath.Substring(lastSlashPos + 1, printerPath.Length - lastSlashPos - 1);

                printServer = new PrintServer(printerServerName);
            }

            Logging.Debug($"Checking Printer Status on '{printerServerName}', printer '{printerName}'");

            var printQueue = printServer.GetPrintQueue(printerName);

            printerQueueStatus = PrinterQueueStatus(printQueue);

            Logging.Trace($"Printer Queue Status: {printerQueueStatus}");

            return !(printQueue.IsOffline || printQueue.IsInError);
        }

        public string MapToFriendlyName(string printerName)
        {
            if (_printerMapping.ContainsValue(printerName))
                return _printerMapping.FirstOrDefault(x => x.Value.Equals(printerName)).Key;

            return printerName;
        }

        public string MapToSystemName(string friendlyName)
        {
            if (_printerMapping.ContainsKey(friendlyName))
                return _printerMapping[friendlyName];

            return friendlyName;
        }

        public string GetFullPrinterName(string printerName)
        {
            var systemName = MapToSystemName(printerName);

            if (systemName.Equals(printerName, StringComparison.Ordinal))
                return printerName;

            return $"{printerName} ({systemName})";
        }

        public List<string> GetPrinters(string printers, Dictionary<string, string> printerMappings)
        {
            return new List<string>(_printerMapping.Keys);
        }
    }
}
