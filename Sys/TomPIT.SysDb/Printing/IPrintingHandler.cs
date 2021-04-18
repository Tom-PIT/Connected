using System;
using System.Collections.Generic;
using TomPIT.Cdn;

namespace TomPIT.SysDb.Printing
{
	public interface IPrintingHandler
	{
		void Insert(Guid token, DateTime created, Guid component, PrintJobStatus status, string provider, string arguments, string user, long serialNumber, string category);
		IPrintJob Select(Guid token);
		List<IPrintJob> QueryJobs();

		void Delete(Guid token);
		void Update(Guid token, PrintJobStatus status, string error);

		void InsertSpooler(Guid token, DateTime created, string mime, string printer, string content);
		IPrintSpoolerJob SelectSpooler(Guid token);
		void DeleteSpooler(IPrintSpoolerJob job);

		List<ISerialNumber> QuerySerialNumbers();
		void InsertSerialNumber(string category, long serialNumber);
		void UpdateSerialNumber(ISerialNumber serialNumber);
		ISerialNumber SelectSerialNumber(string category);
	}
}
