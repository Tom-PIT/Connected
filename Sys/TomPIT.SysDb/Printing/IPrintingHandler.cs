using System;
using TomPIT.Cdn;

namespace TomPIT.SysDb.Printing
{
	public interface IPrintingHandler
	{
		void Insert(Guid token, DateTime created, Guid component, PrintJobStatus status, string provider, string arguments);
		IPrintJob Select(Guid token);

		void Delete(Guid token);
		void Update(Guid token, PrintJobStatus status, string error);
	}
}
