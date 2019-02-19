using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class MicroServiceManagementController : SysController
	{
		[HttpPost]
		public void Insert()
		{
			var body = FromBody();

			var name = body.Required<string>("name");
			var microService = body.Required<Guid>("microService");
			var status = body.Required<MicroServiceStatus>("status");
			var resourceGroup = body.Required<Guid>("resourceGroup");
			var template = body.Required<Guid>("template");
			var meta = body.Required<string>("meta");

			DataModel.MicroServices.Insert(microService, name, status, resourceGroup, template, meta);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var name = body.Required<string>("name");
			var status = body.Required<MicroServiceStatus>("status");
			var template = body.Optional<Guid>("template", Guid.Empty);
			var resourceGroup = body.Required<Guid>("resourceGroup");
			var package = body.Required<Guid>("package");

			DataModel.MicroServices.Update(microService, name, status, template, resourceGroup, package);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");

			DataModel.MicroServices.Delete(microService);
		}

		[HttpGet]
		public List<IMicroService> Query(Guid resourceGroup)
		{
			return DataModel.MicroServices.Query(resourceGroup);
		}

		//[HttpGet]
		//public string CreateMicroServiceMeta(Guid microService)
		//{
		//	var parameters = new JObject
		//	{
		//		{"password",string.Empty },
		//		{"microService", microService }
		//	};

		//	return Shell.GetService<ICryptographyService>().Encrypt(this, JsonConvert.SerializeObject(parameters));
		//}

		[HttpGet]
		public List<IMicroServiceString> QueryStrings(Guid microService)
		{
			return DataModel.MicroServiceStrings.Query(microService);
		}
	}
}
