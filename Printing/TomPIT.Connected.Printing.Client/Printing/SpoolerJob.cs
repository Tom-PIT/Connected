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
    }
}
