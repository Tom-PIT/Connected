namespace TomPIT.UI.Theming.Loggers
{
	using System;
	using TomPIT.UI.Theming.Configuration;

	public class ConsoleLogger : Logger
    {
        public ConsoleLogger(LogLevel level) : base(level) { }

        public ConsoleLogger(LessConfiguration config) : this(config.LogLevel)
        {

        }

        protected override void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}