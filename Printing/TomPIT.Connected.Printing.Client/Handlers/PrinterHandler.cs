/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System.Collections.Generic;
using System.Text;

namespace TomPIT.Connected.Printing.Client.Handlers
{
    internal class PrinterHandler
    {
        private Dictionary<string, string> _printerMapping;

        public PrinterHandler(Dictionary<string, string> printerMappings)
        {

            _printerMapping = printerMappings ?? new Dictionary<string, string>();
        }

        public string MapToSystemName(string friendlyName)
        {
            if (_printerMapping.ContainsKey(friendlyName))
                return _printerMapping[friendlyName];

            return friendlyName;
        }

        public List<string> GetPrinters()
        {
            return new List<string>(_printerMapping.Keys);
        }
    }
}
