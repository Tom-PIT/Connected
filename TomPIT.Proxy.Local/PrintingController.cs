using System;
using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Serialization;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class PrintingController : IPrintingController
	{
		public void Delete(Guid token)
		{
			DataModel.Printing.Delete(token);
		}

		public Guid Insert(string provider, IPrinter printer, Guid component, object arguments, string user, string category, int copyCount)
		{
			var args = new Dictionary<string, object>();

			if (arguments is not null)
				args.Add("arguments", Serializer.Serialize(arguments));

			args.Add("printer", Serializer.Serialize(printer));

			return DataModel.Printing.Insert(component, provider, Serializer.Serialize(args), user, category, copyCount);
		}

		public IPrintJob Select(Guid token)
		{
			return DataModel.Printing.Select(token);
		}

		public void Update(Guid token, PrintJobStatus status, string error)
		{
			DataModel.Printing.Update(token, status, error);
		}
	}
}
