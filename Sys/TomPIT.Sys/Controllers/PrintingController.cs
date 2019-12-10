using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Cdn;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class PrintingController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");
			var provider = body.Required<string>("provider");
			var arguments = body.Optional<string>("arguments", null);

			return DataModel.Printing.Insert(component, provider, arguments);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			DataModel.Printing.Delete(token);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");
			var status = body.Required<PrintJobStatus>("status");
			var error = body.Optional<string>("error", string.Empty);

			DataModel.Printing.Update(token, status, error);
		}

		[HttpPost]
		public IPrintJob Select()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.Printing.Select(token);
		}
	}
}
