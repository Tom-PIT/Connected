using System;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Cdn
{
	internal class PrintingService : TenantObject, IPrintingService
	{
		public PrintingService(ITenant tenant) : base(tenant)
		{

		}

		public void Delete(Guid token)
		{
			var u = Tenant.CreateUrl("Printing", "Delete");
			var e = new JObject
			{
				{"token", token }
			};

			Tenant.Post(u, e);
		}

		public Guid Insert(string provider, IPrinter printer, Guid component, object arguments, string user, string category, int copyCount)
		{
			var u = Tenant.CreateUrl("Printing", "Insert");
			var args = new JObject();

			if (arguments != null)
				args.Add("arguments", Serializer.Serialize(arguments));

			args.Add("printer", Serializer.Serialize(printer));

			var e = new JObject
			{
				{"provider", provider },
				{"component", component },
				{"user", user },
				{"category", category },
				{"copyCount", copyCount },
				{"arguments",  Serializer.Serialize(args)}
			};

			return Tenant.Post<Guid>(u, e);
		}

		public void Update(Guid token, PrintJobStatus status, string error)
		{
			var u = Tenant.CreateUrl("Printing", "Update");
			var e = new JObject
			{
				{"token", token },
				{"status", status.ToString() },
				{"error", error }
			};

			Tenant.Post(u, e);
		}

		public IPrintJob Select(Guid token)
		{
			var u = Tenant.CreateUrl("Printing", "Select");
			var e = new JObject
			{
				{"token", token }
			};

			return Tenant.Post<PrintJob>(u, e);
		}
	}
}
