using System;
using TomPIT.Cdn;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Printing;

namespace TomPIT.SysDb.Sql.Printing
{
	internal class PrintingHandler : IPrintingHandler
	{
		public void Delete(Guid token)
		{
			var w = new Writer("tompit.print_job_del");

			w.CreateParameter("@token", token);

			w.Execute();
		}

		public void DeleteSpooler(IPrintSpoolerJob job)
		{
			var w = new Writer("tompit.print_spooler_del");

			w.CreateParameter("@id", job.GetId());

			w.Execute();
		}

		public void Insert(Guid token, DateTime created, Guid component, PrintJobStatus status, string provider, string arguments)
		{
			var w = new Writer("tompit.print_job_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@created", created);
			w.CreateParameter("@status", status);
			w.CreateParameter("@provider", provider, true);
			w.CreateParameter("@component", component);
			w.CreateParameter("@arguments", arguments, true);

			w.Execute();
		}

		public void InsertSpooler(Guid token, DateTime created, string mime, string printer, string content)
		{
			var w = new Writer("tompit.print_spooler_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@created", created);
			w.CreateParameter("@mime", mime);
			w.CreateParameter("@printer", printer);
			w.CreateParameter("@content", Convert.FromBase64String(content));

			w.Execute();
		}

		public IPrintJob Select(Guid token)
		{
			var r = new Reader<PrintJob>("tompit.print_job_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public IPrintSpoolerJob SelectSpooler(Guid token)
		{
			var r = new Reader<PrintSpoolerJob>("tompit.print_spooler_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(Guid token, PrintJobStatus status, string error)
		{
			var w = new Writer("tompit.print_job_upd");

			w.CreateParameter("@token", token);
			w.CreateParameter("@status", status);
			w.CreateParameter("@error", error, true);

			w.Execute();
		}
	}
}
