/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
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
                foreach(var mapping in mappings)
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
            var printerList = new List<string>();

            if (printers.Equals(Constants.PrintersSetDefaultPrinter, StringComparison.OrdinalIgnoreCase))
            {
                var printerSettings = new PrinterSettings();
                printerList.Add(printerSettings.PrinterName);
            }
            else if (printers.Equals(Constants.PrintersSetInstalledPrinters, StringComparison.OrdinalIgnoreCase))
            {
                var result = new List<string>();

                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    printerList.Add(printerName);
                }
            }
            else if (printers.Equals(Constants.PrintersSetList, StringComparison.OrdinalIgnoreCase))
            {
                printerList.AddRange(Settings.PrinterList);
            }
            else
            {
                printerList.AddRange(printers.Split(';').ToList());
            }

            SetMappings(printerList, printerMappings);
            return new List<string>(_printerMapping.Keys);
        }
    }
}
