using System;
using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Serialization;

namespace TomPIT.Proxy.Remote
{
	internal class PrintingController : IPrintingController
	{
		private const string Controller = "Printing";

		public void Delete(Guid token)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
			{
				token
			});
		}

		public Guid Insert(string provider, IPrinter printer, Guid component, object arguments, string user, string category, int copyCount)
		{
			var u = Connection.CreateUrl(Controller, "Insert");
			var args = new Dictionary<string, object>();

			if (arguments is not null)
				args.Add("arguments", Serializer.Serialize(arguments));

			args.Add("printer", Serializer.Serialize(printer));

			return Connection.Post<Guid>(u, new
			{
				provider,
				component,
				user,
				category,
				copyCount,
				arguments = args
			});
		}

		public IPrintJob Select(Guid token)
		{
			return Connection.Post<PrintJob>(Connection.CreateUrl(Controller, "Select"), new
			{
				token
			});
		}

		public void Update(Guid token, PrintJobStatus status, string error)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Update"), new
			{
				token,
				status = status.ToString(),
				error
			});
		}
	}
}
