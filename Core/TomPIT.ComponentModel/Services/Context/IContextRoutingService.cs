using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Environment;

namespace TomPIT.Services.Context
{
	public interface IContextRoutingService
	{
		string GetServer(InstanceType type, InstanceVerbs verbs);
		string ApplicationUrl(string route);
		string Absolute(string url);
		string Resource(IUrlHelper helper, Guid blob);
		string Avatar(Guid user);

		void NotFound();
		void Forbidden();
		void Redirect(string url);
		void BadRequest();
	}
}
