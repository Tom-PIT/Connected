/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Connected.Printing.Client.Printing
{
    internal class SpoolerJob
    {
        public Guid Token { get; set; }

        public string Mime { get; set; }

        public string Content { get; set; }

        public string Printer { get; set; }

        public override string ToString()
        {
            return $"Job = {Token}, Mime Type = {Mime}, Content Length = {Content.Length * 3 / 4}, Printer = {Printer}";
        }
    }
}
