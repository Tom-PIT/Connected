using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

using TomPIT.Environment;
using TomPIT.Ide.Controllers;
using TomPIT.Ide.Models;
using TomPIT.Management.Environment;
using TomPIT.Management.Models;
using TomPIT.Sys.Model.Environment;

namespace TomPIT.Management.Controllers
{
	[Authorize(Roles = "Full Control")]
	public class ManagementController : IdeControllerBase
	{
		protected override IdeModelBase CreateModel()
		{
			var r = new HomeModel();

			r.Initialize(this, null);

			if (string.IsNullOrWhiteSpace(Request.ContentType)
				 || Request.ContentType.Contains("application/json"))
				r.RequestBody = FromBody();

			r.Databind();

			return r;
		}

		[HttpPost]
		public void SetEndpoints()
		{
			var model = CreateModel();

			var endpoints = model.RequestBody.Required<JArray>("endpoints").ToObject<List<InstanceEndpoint>>();

			var endpointInstanceService = Tenant.GetService<IInstanceEndpointService>();
			var managementService = Tenant.GetService<IInstanceEndpointManagementService>();

			var existingEndpoints = endpointInstanceService.Query();

			foreach (var endpoint in existingEndpoints)
				managementService.Delete(endpoint.Token);

			foreach (var endpoint in endpoints)
				managementService.Insert(endpoint.Name, endpoint.Features, endpoint.Url, endpoint.ReverseProxyUrl, endpoint.Status, endpoint.Verbs);
		}
	}
}
