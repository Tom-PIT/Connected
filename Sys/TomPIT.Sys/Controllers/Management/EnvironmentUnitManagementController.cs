using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class EnvironmentUnitManagementController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();

			var name = body.Required<string>("name");
			var parent = body.Optional<Guid>("parent", Guid.Empty);
			var ordinal = body.Required<int>("ordinal");

			return DataModel.EnvironmentUnits.Insert(name, parent, ordinal);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");
			var name = body.Required<string>("name");
			var parent = body.Optional<Guid>("parent", Guid.Empty);
			var ordinal = body.Required<int>("ordinal");

			DataModel.EnvironmentUnits.Update(token, name, parent, ordinal);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");

			DataModel.EnvironmentUnits.Delete(token);
		}
	}
}
