/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;

namespace TomPIT.Connected.Printing.Client.Handlers
{
    internal class EnvironmentHandler
    {
        public static List<string> GetPrinters(string printers)
        {
            if (printers.Equals(Constants.PrintersSetDefaultPrinter, StringComparison.OrdinalIgnoreCase))
            {
            var printerSettings = new PrinterSettings();
                return new List<string>() { printerSettings.PrinterName };
            }
            else if (printers.Equals(Constants.PrintersSetInstalledPrinters, StringComparison.OrdinalIgnoreCase))
            {
                var result = new List<string>();

                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    result.Add(printerName);
                }

                return result;
            }

            return printers.Split(';').ToList();
        }
    }
}
