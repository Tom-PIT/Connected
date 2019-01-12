using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.SysDb.Environment
{
	public interface IEnvironmentHandler
	{
		/*
		 * Instance endpoints
		 */
		void InsertInstanceEndpoint(Guid token, InstanceType type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs);
		void UpdateInstanceEndpoint(IInstanceEndpoint item, InstanceType type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs);
		void DeleteInstanceEndpoint(IInstanceEndpoint item);

		IInstanceEndpoint SelectInstanceEndpoint(Guid token);
		List<IInstanceEndpoint> QueryInstanceEndpoints();
		/*
		 * Environment units
		 */
		void InsertEnvironmentUnit(Guid token, string name, IEnvironmentUnit parent, int ordinal);
		void UpdateEnvironmentUnit(IEnvironmentUnit item, string name, IEnvironmentUnit parent, int ordinal);
		void UpdateEnvironmentUnits(List<EnvironmentUnitBatchDescriptor> items);
		void DeleteEnvironmentUnit(IEnvironmentUnit item);

		List<IEnvironmentUnit> QueryEnvironmentUnits();
		IEnvironmentUnit SelectEnvironmentUnit(Guid token);
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
