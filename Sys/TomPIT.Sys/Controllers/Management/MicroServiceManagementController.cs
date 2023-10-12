using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;

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
			var supportedStages = body.Required<MicroServiceStages>("supportedStages");
			var resourceGroup = body.Required<Guid>("resourceGroup");
			var template = body.Required<Guid>("template");
			var version = body.Optional("version", string.Empty);
			var commit = body.Optional("commit", string.Empty);

			DataModel.MicroServices.Insert(microService, name, supportedStages, resourceGroup, template, version, commit);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var name = body.Required<string>("name");
			var supportedStages = body.Required<MicroServiceStages>("supportedStages");
			var template = body.Optional("template", Guid.Empty);
			var resourceGroup = body.Required<Guid>("resourceGroup");
			var version = body.Optional("version", string.Empty);
			var commit = body.Optional("commit", string.Empty);

			DataModel.MicroServices.Update(microService, name, supportedStages, template, resourceGroup, version, commit);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");

			DataModel.MicroServices.Delete(microService);
		}

		[HttpGet]
		public ImmutableList<IMicroService> Query(Guid resourceGroup)
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
	}
}