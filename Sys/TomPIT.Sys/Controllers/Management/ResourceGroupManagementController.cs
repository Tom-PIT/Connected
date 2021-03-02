using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Model;
using TomPIT.SysDb.Environment;

namespace TomPIT.Sys.Controllers.Management
{
	public class ResourceGroupManagementController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();

			var name = body.Required<string>("name");
			var storageProvider = body.Required<Guid>("storageProvider");
			var connectionString = body.Optional("connectionString", string.Empty);

			return DataModel.ResourceGroups.Insert(name, storageProvider, connectionString);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");
			var name = body.Required<string>("name");
			var storageProvider = body.Required<Guid>("storageProvider");
			var connectionString = body.Optional("connectionString", string.Empty);

			DataModel.ResourceGroups.Update(token, name, storageProvider, connectionString);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");

			DataModel.ResourceGroups.Delete(token);
		}

		[HttpGet]
		public ImmutableList<IServerResourceGroup> Query()
		{
			return DataModel.ResourceGroups.Query();
		}
	}
}
