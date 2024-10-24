﻿using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
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
			var status = body.Required<MicroServiceStatus>("status");
			var resourceGroup = body.Required<Guid>("resourceGroup");
			var template = body.Required<Guid>("template");
			var meta = body.Required<string>("meta");
			var version = body.Optional("version", string.Empty);

			DataModel.MicroServices.Insert(microService, name, status, resourceGroup, template, meta, version);
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
			var package = body.Optional("package", Guid.Empty);
			var plan = body.Optional("plan", Guid.Empty);
			var updateStatus = body.Required<UpdateStatus>("updateStatus");
			var commitStatus = body.Required<CommitStatus>("commitStatus");

			DataModel.MicroServices.Update(microService, name, status, template, resourceGroup, package, plan, updateStatus, commitStatus);
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

		[HttpGet]
		public ImmutableList<IMicroServiceString> QueryStrings(Guid microService)
		{
			return DataModel.MicroServiceStrings.Query(microService);
		}
	}
}