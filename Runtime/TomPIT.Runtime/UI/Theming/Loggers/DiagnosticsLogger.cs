namespace TomPIT.UI.Theming.Loggers
{
	public class DiagnosticsLogger : Logger
    {
        public DiagnosticsLogger(LogLevel level) : base(level)
        {
        }

        protected override void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}