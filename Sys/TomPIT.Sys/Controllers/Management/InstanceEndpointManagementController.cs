using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Environment;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
	public class InstanceEndpointManagementController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();

			var instanceType = body.Required<InstanceType>("type");
			var name = body.Required<string>("name");
			var url = body.Optional("url", string.Empty);
			var reverseProxyUrl = body.Optional("reverseProxyUrl", string.Empty);
			var status = body.Required<InstanceStatus>("status");
			var verbs = body.Required<InstanceVerbs>("verbs");

			return DataModel.InstanceEndpoints.Insert(instanceType, name, url, reverseProxyUrl, status, verbs);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");
			var instanceType = body.Required<InstanceType>("type");
			var name = body.Required<string>("name");
			var url = body.Optional("url", string.Empty);
			var reverseProxyUrl = body.Optional("reverseProxyUrl", string.Empty);
			var status = body.Required<InstanceStatus>("status");
			var verbs = body.Required<InstanceVerbs>("verbs");

			DataModel.InstanceEndpoints.Update(token, instanceType, name, url, reverseProxyUrl, status, verbs);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");

			DataModel.InstanceEndpoints.Delete(token);
		}
	}
}
