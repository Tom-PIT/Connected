/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.IO;
using System.Text;
#if CONSOLE
using TomPIT.Connected.Printing.Client.Handlers;
#endif
using static System.Environment;

namespace TomPIT.Connected.Printing.Client
{
    internal enum LoggingLevel
    {
        Off,
        Fatal,
        Error,
        Warning,
        Info,
        Debug,
        Trace, 
        All
    }

    internal enum ExceptionLoggingLevel
    {
        Off,
        ErrorMessage,
        StackTrace
    }

    internal static class Logging
    {
        public static LoggingLevel Level { get; set; } = LoggingLevel.Error;
        public static ExceptionLoggingLevel ExceptionLogging { get; set; } = ExceptionLoggingLevel.ErrorMessage;

        private static void WriteFile(string text, bool isErrorFile)
        {
#if CONSOLE
            if (isErrorFile)
                ConsoleHandler.Error(text);
            else
                ConsoleHandler.Info(text);
#endif
            var commonApps = Environment.GetFolderPath(SpecialFolder.CommonApplicationData);
            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            var logFolder = Path.Combine(commonApps, "TomPIT", appName, "logs");

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            var fileName = Path.Combine(logFolder, $"{DateTime.Now:yyyy_MM_dd}{(isErrorFile ? ".error" : "")}.log");

            try
            {
                using (StreamWriter streamWriter = new StreamWriter(fileName, true))
                {
                    streamWriter.WriteLine(text);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            catch { }
        }

        private static void Log(string text, LoggingLevel severity)
        { 
            if (severity <= Level)
            {
                var sb = new StringBuilder().Append(DateTime.Now.ToString("HH:mm:ss.fff")).Append("|").Append(severity.ToString().ToUpper()).Append("|").Append(text);
                WriteFile(sb.ToString(), (severity == LoggingLevel.Error) || (severity == LoggingLevel.Fatal));
            }
        }

        public static void Exception(Exception ex, LoggingLevel level = LoggingLevel.Error,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (ExceptionLogging == ExceptionLoggingLevel.Off)
                return;

            var source = $"{Path.GetFileName(sourceFilePath)}/{memberName} Line: {sourceLineNumber}";

            var sb = new StringBuilder();
            sb.Append(" Source: ").AppendLine(source);
            sb.Append(" Error: ").AppendLine(ex.Message);

            if (ExceptionLogging >= ExceptionLoggingLevel.StackTrace)
            {
                sb.Append(" StackTrace: ").AppendLine(ex.StackTrace);
            }

            string padding = " ";
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.Append("   Inner Exception Error: ").AppendLine(ex.Message);
                if (ExceptionLogging >= ExceptionLoggingLevel.StackTrace)
                {
                    sb.Append("   StackTrace: ").AppendLine(ex.StackTrace);
                }
            }

            padding += " ";
            WriteFile(sb.ToString(), true);
        }

        public static void Fatal(string text)
        {
            Log(text, LoggingLevel.Fatal);
        }

        public static void Error(string text)
        {
            Log(text, LoggingLevel.Error);
        }

        public static void Warning(string text)
        {
            Log(text, LoggingLevel.Warning);
        }

        public static void Info(string text)
        {
            Log(text, LoggingLevel.Info);
        }

        public static void Debug(string text)
        {
            Log(text, LoggingLevel.Debug);
        }

        public static void Trace(string text)
        {
            Log(text, LoggingLevel.Trace);
        }
    }
}
