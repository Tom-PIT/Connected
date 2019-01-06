using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Net;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public interface IRoutingService
	{
		string GetServer(InstanceType type, InstanceVerbs verbs);

		string Absolute(string url);
		string Resource(IUrlHelper helper, Guid blob);
		string Avatar(Guid user);
	}
}
