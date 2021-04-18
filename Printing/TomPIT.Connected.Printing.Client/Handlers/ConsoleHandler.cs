/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.Diagnostics;

namespace TomPIT.Connected.Printing.Client.Handlers
{
    internal static class ConsoleHandler
    {
        private static string DoEllipsis(string text, int maxLength = 120)
        {
            if (text.Length > maxLength)
            {
                var trimmed = text.Trim();

                var trimmedLength = trimmed.Length;

                var lastPart = trimmed.Substring(trimmedLength - 10, 10);

                var firstPart = trimmed.Substring(0, Math.Min(maxLength, trimmedLength - 10));

                return $"{firstPart}...{lastPart}";
            }
            return text;
        }

        [Conditional("CONSOLE")]
        private static void TextToConsole(string text, ConsoleColor color, string type = "")
        {
            Console.ForegroundColor = color;

            string textToPrint = string.IsNullOrEmpty(type) ? text : $"{type}: {text}";
            Console.WriteLine(DoEllipsis(textToPrint));

            Console.ResetColor();

            if (Console.CursorTop > 30)
            {
                //Console.Clear();
            }
        }

        [Conditional("CONSOLE")]
        public static void Error(string errorText)
        {
            TextToConsole(errorText, ConsoleColor.Red, "ERROR");
        }

        [Conditional("CONSOLE")]
        public static void Info(string info)
        {
            TextToConsole(info, ConsoleColor.Green, "INFO");
        }

        [Conditional("CONSOLE")]
        public static void Debug(string info)
        {
            TextToConsole(info, ConsoleColor.Gray, "DEBUG");
        }

    }
}
