using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Printing;

namespace TomPIT.SysDb.Sql.Printing
{
	internal class PrintingHandler : IPrintingHandler
	{
		public void DeleteSpooler(IPrintSpoolerJob job)
		{
			using var w = new Writer("tompit.print_spooler_del");

			w.CreateParameter("@id", job.GetId());

			w.Execute();
		}

		public void Update(List<IPrintJob> jobs)
		{
			using var w = new Writer("tompit.print_job_upd");
			var items = new JArray();

			foreach (var item in jobs)
			{
				var jo = new JObject
				{
					{"created", item.Created },
					{"token", item.Token },
					{"status", (int)item.Status },
					{"component", item.Component },
					{"copy_count", item.CopyCount }
				};

				if (!string.IsNullOrEmpty(item.Error))
					jo.Add("error", item.Error);

				if (!string.IsNullOrEmpty(item.Arguments))
					jo.Add("arguments", item.Arguments);

				if (!string.IsNullOrEmpty(item.Provider))
					jo.Add("provider", item.Provider);

				if (!string.IsNullOrEmpty(item.User))
					jo.Add("user", item.User);

				if (item.SerialNumber > 0)
					jo.Add("serial_number", item.SerialNumber);

				if (!string.IsNullOrEmpty(item.Category))
					jo.Add("category", item.Category);

				items.Add(jo);
			};

			w.CreateParameter("@items", items);

			w.Execute();
		}

		public void InsertSerialNumber(string category, long serialNumber)
		{
			using var w = new Writer("tompit.print_job_serial_number_ins");

			w.CreateParameter("@category", category);
			w.CreateParameter("@serial_number", serialNumber);

			w.Execute();
		}

		public void InsertSpooler(Guid token, DateTime created, string mime, string printer, string content, Guid identity, int copyCount = 1)
		{
			using var w = new Writer("tompit.print_spooler_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@created", created);
			w.CreateParameter("@mime", mime);
			w.CreateParameter("@printer", printer);
			w.CreateParameter("@content", Convert.FromBase64String(content));
			w.CreateParameter("@identity", identity);
			w.CreateParameter("@copy_count", copyCount);

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

		public void UpdateSerialNumber(ISerialNumber serialNumber)
		{
			using var w = new Writer("tompit.print_job_serial_number_upd");

			w.CreateParameter("@id", serialNumber.GetId());
			w.CreateParameter("@serial_number", serialNumber);

			w.Execute();
		}
	}
}
