using System;

namespace TomPIT.Cdn
{
	public interface IPrintingService
	{
		Guid Insert(string provider, IPrinter printer, Guid component, object arguments, string user, string category);
		IPrintJob Select(Guid token);
		void Delete(Guid token);
		void Update(Guid token, PrintJobStatus status, string error);
	}
}
