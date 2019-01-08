using System;

namespace TomPIT.Environment
{
	public interface IInstanceEndpointManagementService
	{
		Guid Insert(string name, InstanceType type, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs);
		void Update(Guid instance, string name, InstanceType type, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs);
		void Delete(Guid instance);
	}
}
