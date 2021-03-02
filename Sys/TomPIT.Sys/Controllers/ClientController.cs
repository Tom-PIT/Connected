using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Environment;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class ClientController : SysController
	{
		[HttpGet]
		public ImmutableList<IClient> Query()
		{
			return DataModel.Clients.Query();
		}

		[HttpPost]
		public IClient Select()
		{
			var body = FromBody();
			var token = body.Optional("token", string.Empty);

			return DataModel.Clients.Select(token);
		}

		[HttpPost]
		public string Insert()
		{
			var body = FromBody();
			var name = body.Required<string>("name");
			var type = body.Optional("type", string.Empty);
			var status = body.Optional("status", ClientStatus.Enabled);

			return DataModel.Clients.Insert(name, status, type);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();
			var token = body.Required<string>("token");
			var name = body.Required<string>("name");
			var status = body.Optional("status", ClientStatus.Enabled);
			var type = body.Optional("type", string.Empty);

			DataModel.Clients.Update(token, name, status, type);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();
			var token = body.Required<string>("token");

			DataModel.Clients.Delete(token);
		}
	}
}
