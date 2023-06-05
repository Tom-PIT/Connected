using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TomPIT.Diagnostics
{
   public static class EventLog
   {
      private static object WriteLock = new object();
      private static void Write(string message, EventLogEntryType type)
      {
         try
         {
            lock (WriteLock)
            {
               //   using System.Diagnostics.EventLog eventLog = new("Application");
               //   eventLog.Source = "TomPIT.Sys";
               //   eventLog.WriteEntry(message, type);
               var fileName = $"dump_{DateTimeOffset.UtcNow.Year}_{DateTimeOffset.UtcNow.Month}_{DateTimeOffset.UtcNow.Day}.txt";
               var directory = Path.Combine("logs");
               var path = Path.Combine(directory, fileName);

               if (!Directory.Exists(directory))
                  Directory.CreateDirectory(directory);

               File.AppendAllLines(path, new[] { message });
            }
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