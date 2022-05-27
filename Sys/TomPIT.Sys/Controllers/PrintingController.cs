using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Cdn;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class PrintingController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");
			var provider = body.Optional("provider", string.Empty);
			var user = body.Optional("user", string.Empty);
			var arguments = body.Optional<string>("arguments", null);
			var category = body.Optional("category", string.Empty);
			var copyCount = body.Optional("copyCount", 1);

			return DataModel.Printing.Insert(component, provider, arguments, user, category, copyCount);
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

		[HttpPost]
		public long NextSerialNumber()
		{
			var body = FromBody();
			var category = body.Required<string>("category");

			return DataModel.Printing.NextSerialNumber(category);
		}
	}
}
