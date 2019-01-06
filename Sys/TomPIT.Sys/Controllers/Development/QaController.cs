using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Development;

namespace TomPIT.Sys.Controllers.Development
{
	public class QaController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();

			var api = body.Required<string>("api");
			var title = body.Required<string>("title");
			var description = body.Optional("description", string.Empty);
			var b = body.Optional("body", string.Empty);
			var tags = body.Required<string>("tags");
			var identifier = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Development.QA.Api.Insert(identifier, title, description, api, b, tags);

			return identifier;
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var identifier = body.Required<Guid>("identifier");
			var api = body.Required<string>("api");
			var title = body.Required<string>("title");
			var description = body.Optional("description", string.Empty);
			var b = body.Optional("body", string.Empty);
			var tags = body.Required<string>("tags");

			Shell.GetService<IDatabaseService>().Proxy.Development.QA.Api.Update(identifier, title, description, api, b, tags);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var identifier = body.Required<Guid>("identifier");

			Shell.GetService<IDatabaseService>().Proxy.Development.QA.Api.Delete(identifier);
		}

		[HttpGet]
		public List<IApiTest> Query()
		{
			return Shell.GetService<IDatabaseService>().Proxy.Development.QA.Api.Query();
		}

		[HttpGet]
		public string SelectBody(Guid identifier)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Development.QA.Api.SelectBody(identifier);
		}
	}
}
