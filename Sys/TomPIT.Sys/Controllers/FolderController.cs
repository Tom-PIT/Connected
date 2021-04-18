using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class FolderController : SysController
	{
		[HttpGet]
		public ImmutableList<IFolder> Query()
		{
			return DataModel.Folders.Query();
		}

		[HttpPost]
		public ImmutableList<IFolder> QueryForResourceGroups()
		{
			var body = FromBody<JArray>();

			if (body == null)
				return null;

			var list = new List<string>();

			foreach (JValue i in body)
				list.Add(i.Value<string>());

			return DataModel.Folders.Query(list);
		}

		[HttpPost]
		public ImmutableList<IFolder> QueryForMicroService()
		{
			var body = FromBody();
			var ms = body.Required<Guid>("microService");

			return DataModel.Folders.Query(ms);
		}

		[HttpGet]
		public IFolder SelectByToken(Guid token)
		{
			return DataModel.Folders.Select(token);
		}

		[HttpGet]
		public IFolder Select(Guid microService, string name)
		{
			return DataModel.Folders.Select(microService, name);
		}
	}
}
