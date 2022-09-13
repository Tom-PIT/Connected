using System;
using System.Collections.Generic;
using TomPIT.Cdn;

namespace TomPIT.SysDb.Printing
{
	public interface IPrintingHandler
	{
		void Update(List<IPrintJob> items);
		List<IPrintJob> QueryJobs();

		void InsertSpooler(Guid token, DateTime created, string mime, string printer, string content, Guid identity, int copyCount = 1);
		IPrintSpoolerJob SelectSpooler(Guid token);
		void DeleteSpooler(IPrintSpoolerJob job);

		List<ISerialNumber> QuerySerialNumbers();
		void InsertSerialNumber(string category, long serialNumber);
		void UpdateSerialNumber(ISerialNumber serialNumber);
		ISerialNumber SelectSerialNumber(string category);
	}
}
