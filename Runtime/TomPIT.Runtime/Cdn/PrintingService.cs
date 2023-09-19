using System;
using TomPIT.Connectivity;

namespace TomPIT.Cdn
{
	internal class PrintingService : TenantObject, IPrintingService
	{
		public PrintingService(ITenant tenant) : base(tenant)
		{

		}

		public void Delete(Guid token)
		{
			Instance.SysProxy.Printing.Delete(token);
		}

		public Guid Insert(string provider, IPrinter printer, Guid component, object arguments, string user, string category, int copyCount)
		{
			return Instance.SysProxy.Printing.Insert(provider, printer, component, arguments, user, category, copyCount);
		}

		public void Update(Guid token, PrintJobStatus status, string error)
		{
			Instance.SysProxy.Printing.Update(token, status, error);
		}

		public IPrintJob Select(Guid token)
		{
			return Instance.SysProxy.Printing.Select(token);
		}
	}
}
