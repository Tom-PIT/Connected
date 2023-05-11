using System;
using TomPIT.Cdn;

namespace TomPIT.Proxy
{
	public interface IPrintingController
	{
		Guid Insert(string provider, IPrinter printer, Guid component, object arguments, string user, string category, int copyCount);
		IPrintJob Select(Guid token);
		void Delete(Guid token);
		void Update(Guid token, PrintJobStatus status, string error);
	}
}
