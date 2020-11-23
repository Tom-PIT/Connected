using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Connected.Printing.Client
{
    internal class Constants
    {
        public const string MimeTypeReport = "devexpress/report";
        public const string MimeTypeJson = "application/json";

        public const string HttpHeaderBearer = "Bearer";

        public const string ServerMethodNameComplete = "Complete";
        public const string ServerMethodNamePing = "Ping";
        public const string ServerMethodNameAddPrinters = "Add";

        public const string RequestPrint = "print";
    }
}
