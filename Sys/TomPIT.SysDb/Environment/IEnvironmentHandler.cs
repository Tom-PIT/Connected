using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.SysDb.Environment
{
	public interface IEnvironmentHandler
	{
		IClientHandler Clients { get; }

		/*
		 * Instance endpoints
		 */
		void InsertInstanceEndpoint(Guid token, InstanceFeatures features, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs);
		void UpdateInstanceEndpoint(IInstanceEndpoint item, InstanceFeatures features, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs, bool locallyHosted = false);
		void DeleteInstanceEndpoint(IInstanceEndpoint item);

		IInstanceEndpoint SelectInstanceEndpoint(Guid token);
		List<IInstanceEndpoint> QueryInstanceEndpoints();
		/*
		 * Resource groups
		 */
		void InsertResourceGroup(Guid token, string name, Guid storageProvider, string connectionString);
		void UpdateResourceGroup(IResourceGroup item, string name, Guid storageProvider, string connectionString);
		void DeleteResourceGroup(IResourceGroup item);

		List<IServerResourceGroup> QueryResourceGroups();
		IServerResourceGroup SelectResourceGroup(Guid token);
		/*
		 * Environment variables
		 */
		void UpdateEnvironmentVariable(string name, string value);
		IEnvironmentVariable SelectEnvironmentVariable(string name);
		List<IEnvironmentVariable> QueryEnvironmentVariables();
		void DeleteEnvironmentVariable(string name);
	}
}
