using System;
using TomPIT.Environment;

namespace TomPIT.Management.Environment
{
	public interface IInstanceEndpointManagementService
	{
		Guid Insert(string name, InstanceFeatures features, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs);
		void Update(Guid instance, string name, InstanceFeatures features, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs);
		void Delete(Guid instance);
	}
}
