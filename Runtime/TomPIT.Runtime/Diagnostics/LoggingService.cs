using Microsoft.Extensions.Configuration;

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
			DumpEnabled = Shell.Configuration.GetSection("diagnostics")?.GetValue<bool>("dumpEnabled") ?? false;
		}
	}
}
