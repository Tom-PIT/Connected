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

		public void Insert(Guid token, DateTime created, Guid component, PrintJobStatus status, string provider, string arguments)
		{
			var w = new Writer("tompit.print_job_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@created", created);
			w.CreateParameter("@status", status);
			w.CreateParameter("@provider", provider);
			w.CreateParameter("@component", component);
			w.CreateParameter("@arguments", arguments, true);

			w.Execute();
		}

		public IPrintJob Select(Guid token)
		{
			var r = new Reader<PrintJob>("tompit.print_job_sel");

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
