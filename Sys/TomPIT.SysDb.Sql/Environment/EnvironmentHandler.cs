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
		private IClientHandler _clients = null;

		public IClientHandler Clients
		{
			get
			{
				return _clients ??= new ClientHandler();
			}
		}
		/*
		 * Instance endpoints
		 */
		public void DeleteInstanceEndpoint(IInstanceEndpoint d)
		{
			using var w = new Writer("tompit.instance_endpoint_del");

			w.CreateParameter("@id", d.GetId());

			w.Execute();
		}

		public void InsertInstanceEndpoint(Guid token, InstanceFeatures features, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			using var w = new Writer("tompit.instance_endpoint_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@type", features);
			w.CreateParameter("@name", name);
			w.CreateParameter("@url", url, true);
			w.CreateParameter("@reverse_proxy_url", reverseProxyUrl, true);
			w.CreateParameter("@status", status);
			w.CreateParameter("@verbs", verbs);

			w.Execute();
		}

		public List<IInstanceEndpoint> QueryInstanceEndpoints()
		{
			using var r = new Reader<InstanceEndpoint>("tompit.instance_endpoint_que");

			return r.Execute().ToList<IInstanceEndpoint>();
		}

		public IInstanceEndpoint SelectInstanceEndpoint(Guid token)
		{
			using var r = new Reader<InstanceEndpoint>("tompit.instance_endpoint_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void UpdateInstanceEndpoint(IInstanceEndpoint target, InstanceFeatures features, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs, bool locallyHosted = false)
		{
			using var w = new Writer("tompit.instance_endpoint_upd");

			w.CreateParameter("@id", target.GetId());
			w.CreateParameter("@url", url, true);
			w.CreateParameter("@status", status);
			w.CreateParameter("@name", name);
			w.CreateParameter("@type", features);
			w.CreateParameter("@verbs", verbs);
			w.CreateParameter("@reverse_proxy_url", reverseProxyUrl, true);
			w.CreateParameter("@locally_hosted", locallyHosted);

			w.Execute();
		}

		/*
		 * Resource groups
		 */
		public void DeleteResourceGroup(IResourceGroup d)
		{
			using var w = new Writer("tompit.resource_group_del");

			w.CreateParameter("@id", d.GetId());

			w.Execute();
		}

		public void InsertResourceGroup(Guid token, string name, Guid storageProvider, string connectionString)
		{
			using var w = new Writer("tompit.resource_group_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@name", name);
			w.CreateParameter("@storage_provider", storageProvider);
			w.CreateParameter("@connection_string", connectionString, true);

			w.Execute();
		}

		public List<IServerResourceGroup> QueryResourceGroups()
		{
			using var r = new Reader<ResourceGroup>("tompit.resource_group_que");

			return r.Execute().ToList<IServerResourceGroup>();
		}

		public IServerResourceGroup SelectResourceGroup(Guid token)
		{
			using var r = new Reader<ResourceGroup>("tompit.resource_group_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void UpdateResourceGroup(IResourceGroup target, string name, Guid storageProvider, string connectionString)
		{
			using var w = new Writer("tompit.resource_group_upd");

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
			using var w = new Writer("tompit.environment_var_mdf");

			w.CreateParameter("@name", name);
			w.CreateParameter("@value", value, true);

			w.Execute();
		}

		public IEnvironmentVariable SelectEnvironmentVariable(string name)
		{
			using var r = new Reader<EnvironmentVariable>("tompit.environment_var_sel");

			r.CreateParameter("@name", name);

			return r.ExecuteSingleRow();
		}

		public List<IEnvironmentVariable> QueryEnvironmentVariables()
		{
			using var r = new Reader<EnvironmentVariable>("tompit.environment_var_que");

			return r.Execute().ToList<IEnvironmentVariable>();
		}

		public void DeleteEnvironmentVariable(string name)
		{
			using var w = new Writer("tompit.environment_var_del");

			w.CreateParameter("@name", name);

			w.Execute();
		}
	}
}
