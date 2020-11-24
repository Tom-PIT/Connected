/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

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

        public const string PrintersSetDefaultPrinter = "default";
        public const string PrintersSetInstalledPrinters = "installed";
    }
}
