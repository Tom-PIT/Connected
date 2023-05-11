using TomPIT.Connectivity;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Diagnostics
{
	internal class LoggingService : TenantObject, ILoggingService
	{
		private readonly bool _dumpEnabled;

		public LoggingService(ITenant tenant) : base(tenant)
		{
			_dumpEnabled = Shell.GetConfiguration<IClientSys>().Diagnostics.DumpEnabled;
		}

		private bool DumpEnabled => _dumpEnabled;
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
	}
}
