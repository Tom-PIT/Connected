using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
	public class RoleManagementController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();

			var name = body.Required<string>("name");

			return DataModel.Roles.Insert(name);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");
			var name = body.Required<string>("name");

			DataModel.Roles.Update(token, name);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");

			DataModel.Roles.Delete(token);
		}
	}
}
