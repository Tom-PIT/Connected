using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.SysDb.Environment;

namespace TomPIT.SysDb.Sql.Environment
{
	internal class EnvironmentHandler : IEnvironmentHandler
	{
		/*
		 * Instance endpoints
		 */
		public void DeleteInstanceEndpoint(IInstanceEndpoint d)
		{
			var w = new Writer("tompit.instance_endpoint_del");

			w.CreateParameter("@id", d.GetId());

			w.Execute();
		}

		public void InsertInstanceEndpoint(Guid token, InstanceType type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var w = new Writer("tompit.instance_endpoint_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@type", type);
			w.CreateParameter("@name", name);
			w.CreateParameter("@url", url, true);
			w.CreateParameter("@reverse_proxy_url", reverseProxyUrl, true);
			w.CreateParameter("@status", status);
			w.CreateParameter("@verbs", verbs);

			w.Execute();
		}

		public List<IInstanceEndpoint> QueryInstanceEndpoints()
		{
			return new Reader<InstanceEndpoint>("tompit.instance_endpoint_que").Execute().ToList<IInstanceEndpoint>();
		}

		public IInstanceEndpoint SelectInstanceEndpoint(Guid token)
		{
			var r = new Reader<InstanceEndpoint>("tompit.instance_endpoint_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void UpdateInstanceEndpoint(IInstanceEndpoint target, InstanceType type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var w = new Writer("tompit.instance_endpoint_upd");

			w.CreateParameter("@id", target.GetId());
			w.CreateParameter("@url", url, true);
			w.CreateParameter("@status", status);
			w.CreateParameter("@name", name);
			w.CreateParameter("@type", type);
			w.CreateParameter("@verbs", verbs);
			w.CreateParameter("@reverse_proxy_url", reverseProxyUrl, true);

			w.Execute();
		}

		/*
		 * Resource groups
		 */
		public void DeleteResourceGroup(IResourceGroup d)
		{
			var w = new Writer("tompit.resource_group_del");

			w.CreateParameter("@id", d.GetId());

			w.Execute();
		}

		public void InsertResourceGroup(Guid token, string name, Guid storageProvider, string connectionString)
		{
			var w = new Writer("tompit.resource_group_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@name", name);
			w.CreateParameter("@storage_provider", storageProvider);
			w.CreateParameter("@connection_string", connectionString, true);

			w.Execute();
		}

		public List<IServerResourceGroup> QueryResourceGroups()
		{
			return new Reader<ResourceGroup>("tompit.resource_group_que").Execute().ToList<IServerResourceGroup>();
		}

		public IServerResourceGroup SelectResourceGroup(Guid token)
		{
			var r = new Reader<ResourceGroup>("tompit.resource_group_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void UpdateResourceGroup(IResourceGroup target, string name, Guid storageProvider, string connectionString)
		{
			var w = new Writer("tompit.resource_group_upd");

			w.CreateParameter("@id", target.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@storage_provider", storageProvider);
			w.CreateParameter("@connection_string", connectionString, true);

			w.Execute();
		}
		/*
		 * Environment variables
		 */
		public void UpdateEnvironmentVariable(string name, string value)
		{
			var w = new Writer("tompit.environment_var_mdf");

			w.CreateParameter("@name", name);
			w.CreateParameter("@value", value, true);

			w.Execute();
		}

		public IEnvironmentVariable SelectEnvironmentVariable(string name)
		{
			var r = new Reader<EnvironmentVariable>("tompit.environment_var_sel");

			r.CreateParameter("@name", name);

			return r.ExecuteSingleRow();
		}

		public List<IEnvironmentVariable> QueryEnvironmentVariables()
		{
			return new Reader<EnvironmentVariable>("tompit.environment_var_que").Execute().ToList<IEnvironmentVariable>();
		}

		public void DeleteEnvironmentVariable(string name)
		{
			var w = new Writer("tompit.environment_var_del");

			w.CreateParameter("@name", name);

			w.Execute();
		}
	}
}
