using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Controllers;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Controllers
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class PrintingSpoolerController : ServerController
	{
		[HttpPost]
		public IPrintSpoolerJob SelectJob()
		{
			var body = FromBody();
			var id = body.Required<Guid>("id");
			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", "SelectSpooler");

			return MiddlewareDescriptor.Current.Tenant.Post<PrintSpoolerJob>(url, new
			{
				Token = id
			});
		}
	}
}
