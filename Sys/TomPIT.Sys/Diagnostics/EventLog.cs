using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.Sys.Diagnostics
{
    public static class EventLog
    {
        private static void Write(string message, EventLogEntryType type)
        {
            try
            {
                using System.Diagnostics.EventLog eventLog = new("Application");
                eventLog.Source = "TomPIT.Sys";
                eventLog.WriteEntry(message, type);
            }
            catch
            {
                //Oh well...
            }
        }

        private static void WindowsCheck(Action e)
        {
            if (!OperatingSystem.IsWindows())
                return;

            e();
        }

        public static void WriteInfo(string message) => WindowsCheck(() => Write(message, EventLogEntryType.Information));
        public static void WriteError(string message) => WindowsCheck(() => Write(message, EventLogEntryType.Error));
        public static void WriteWarning(string message) => WindowsCheck(() => Write(message, EventLogEntryType.Warning));
    }
}
