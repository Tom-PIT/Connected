using System.Text.Json;
using TomPIT.Connectivity;

namespace TomPIT.Diagnostics
{
	internal class LoggingService : TenantObject, ILoggingService
	{
		private readonly bool _dumpEnabled;

		public LoggingService(ITenant tenant) : base(tenant)
		{
			Initialize();
		}

		public bool DumpEnabled { get; private set; }
		public void Write(ILogEntry d)
		{
			Instance.SysProxy.Logging.Write(d);
		}

		public void Flush()
		{
			Instance.SysProxy.Logging.Flush();
		}

		public void Dump(string text)
		{
			if (!DumpEnabled)
				return;

			Instance.SysProxy.Logging.Dump(text);
		}

		private void Initialize()
		{
			if (!Shell.Configuration.RootElement.TryGetProperty("diagnostics", out JsonElement element))
				return;

			if (!element.TryGetProperty("dumpEnabled", out JsonElement dumpEnabled))
				return;

			DumpEnabled = dumpEnabled.GetBoolean();
		}
	}
}
