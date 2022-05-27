using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Cdn;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Printing;

namespace TomPIT.SysDb.Sql.Printing
{
	internal class PrintingHandler : IPrintingHandler
	{
		public void Delete(Guid token)
		{
			using var w = new Writer("tompit.print_job_del");

			w.CreateParameter("@token", token);

			w.Execute();
		}

		public void DeleteSpooler(IPrintSpoolerJob job)
		{
			using var w = new Writer("tompit.print_spooler_del");

			w.CreateParameter("@id", job.GetId());

			w.Execute();
		}

		public void Insert(Guid token, DateTime created, Guid component, PrintJobStatus status, string provider, string arguments, string user, long serialNumber, string category, int copyCount)
		{
			using var w = new Writer("tompit.print_job_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@created", created);
			w.CreateParameter("@status", status);
			w.CreateParameter("@provider", provider, true);
			w.CreateParameter("@component", component);
			w.CreateParameter("@arguments", arguments, true);
			w.CreateParameter("@user", user, true);
			w.CreateParameter("@serial_number", serialNumber, true);
			w.CreateParameter("@category", category, true);
			w.CreateParameter("@copy_count", copyCount);

			w.Execute();
		}

		public void InsertSerialNumber(string category, long serialNumber)
		{
			using var w = new Writer("tompit.print_job_serial_number_ins");

			w.CreateParameter("@category", category);
			w.CreateParameter("@serial_number", serialNumber);

			w.Execute();
		}

		public void InsertSpooler(Guid token, DateTime created, string mime, string printer, string content, Guid identity)
		{
			using var w = new Writer("tompit.print_spooler_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@created", created);
			w.CreateParameter("@mime", mime);
			w.CreateParameter("@printer", printer);
			w.CreateParameter("@content", Convert.FromBase64String(content));
			w.CreateParameter("@identity", identity);

			w.Execute();
		}

		public List<IPrintJob> QueryJobs()
		{
			using var r = new Reader<PrintJob>("tompit.print_job_que");

			return r.Execute().ToList<IPrintJob>();
		}

		public List<ISerialNumber> QuerySerialNumbers()
		{
			using var r = new Reader<SerialNumberDescriptor>("tompit.print_job_serial_number_que");

			return r.Execute().ToList<ISerialNumber>();
		}

		public IPrintJob Select(Guid token)
		{
			using var r = new Reader<PrintJob>("tompit.print_job_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public ISerialNumber SelectSerialNumber(string category)
		{
			using var r = new Reader<SerialNumberDescriptor>("tompit.print_job_serial_number_sel");

			r.CreateParameter("@category", category);

			return r.ExecuteSingleRow();
		}

		public IPrintSpoolerJob SelectSpooler(Guid token)
		{
			using var r = new Reader<PrintSpoolerJob>("tompit.print_spooler_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(Guid token, PrintJobStatus status, string error)
		{
			using var w = new Writer("tompit.print_job_upd");

			w.CreateParameter("@token", token);
			w.CreateParameter("@status", status);
			w.CreateParameter("@error", error, true);

			w.Execute();
		}

		public void UpdateSerialNumber(ISerialNumber serialNumber)
		{
			using var w = new Writer("tompit.print_job_serial_number_upd");

			w.CreateParameter("@id", serialNumber.GetId());
			w.CreateParameter("@serial_number", serialNumber);

			w.Execute();
		}
	}
}
