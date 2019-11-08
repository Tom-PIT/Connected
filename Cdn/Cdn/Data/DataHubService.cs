using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Data
{
	internal class DataHubService : TenantObject, IDataHubService
	{
		private Lazy<ConcurrentDictionary<Guid, List<EndpointConnection>>> _connectionPointers = new Lazy<ConcurrentDictionary<Guid, List<EndpointConnection>>>();
		public DataHubService(ITenant tenant) : base(tenant)
		{
			Tenant.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			Tenant.GetService<IComponentService>().ComponentRemoved += OnComponentRemoved;
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.DataHub, true) != 0)
				return;

			UpdateComponent(e.Component);
		}

		private void OnComponentRemoved(ITenant sender, ComponentEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.DataHub, true) != 0)
				return;

			RemoveComponent(e.Component);
		}

		private void OnComponentChanged(ITenant sender, ComponentEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.DataHub, true) != 0)
				return;

			UpdateComponent(e.Component);
		}

		private void UpdateComponent(Guid component)
		{
			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component) as IDataHubConfiguration;

			if (config == null)
				RemoveComponent(component);

			foreach (var pointer in ConnectionPointers)
			{
				foreach (var connection in pointer.Value)
				{
					var targets = connection.Descriptors.Where(f => f.Value.Component == component);

					if (targets.Count() == 0)
						continue;

					foreach (var target in targets)
					{
						var policy = FindPolicy(config, target.Value.Policy.Id);

						if (policy == null)
							connection.Descriptors.Remove(target.Key, out _);
						else
							target.Value.Policy = policy;
					}
				}
			}
		}

		private IDataHubEndpointPolicy FindPolicy(IDataHubConfiguration configuration, Guid id)
		{
			foreach (var endpoint in configuration.Endpoints)
			{
				foreach (var policy in endpoint.Policies)
				{
					if (policy.Id == id)
						return policy;
				}
			}

			return null;
		}

		private void RemoveComponent(Guid component)
		{
			foreach (var pointer in ConnectionPointers)
			{
				foreach (var connection in pointer.Value)
				{
					var targets = connection.Descriptors.Where(f => f.Value.Component == component);

					if (targets.Count() == 0)
						continue;

					foreach (var target in targets)
						connection.Descriptors.Remove(target.Key, out _);
				}
			}
		}

		public void Connect(string microService, string dataHub, string connectionId, List<DataHubEndpointSubscriber> endpoints)
		{
			var descriptor = ComponentDescriptor.DataHub(new MiddlewareContext(Tenant.Url), $"{microService}/{dataHub}");

			descriptor.Validate();

			/*
			 * attach all hooks or none
			 * we need to validate all first
			 */
			foreach (var endpoint in endpoints)
			{
				var dep = descriptor.Configuration.Endpoints.FirstOrDefault(f => string.Compare(f.Name, endpoint.Name, true) == 0);

				if (dep == null)
					throw new RuntimeException($"{SR.ErrDataHubEndpointNotFound} ({endpoint.Name})");

				foreach (var policy in endpoint.Policies)
				{
					var policyConfiguration = dep.Policies.FirstOrDefault(f => string.Compare(f.Name, policy.Name, true) == 0);

					if (policyConfiguration == null)
						throw new RuntimeException($"{SR.ErrDataHubEndpointPolicyNotFound} ({microService}/{dataHub}/{endpoint.Name}/{policy.Name})");
				}
			}

			foreach (var endpoint in endpoints)
			{
				var dep = descriptor.Configuration.Endpoints.FirstOrDefault(f => string.Compare(f.Name, endpoint.Name, true) == 0);
				var pointer = new EndpointConnection
				{
					ConnectionId = connectionId,
				};

				foreach (var policy in endpoint.Policies)
				{
					var policyConfiguration = dep.Policies.FirstOrDefault(f => string.Compare(f.Name, policy.Name, true) == 0);

					pointer.Descriptors.TryAdd(policy.Name, new EndpointConnectionDescriptor(Tenant, policyConfiguration, policy.Arguments));
				}

				AttachPointer(dep.Id, pointer);
			}
		}

		public void Disconnect(string connectionId)
		{
			foreach (var endpoint in ConnectionPointers)
			{
				for (var i = endpoint.Value.Count - 1; i >= 0; i--)
				{
					if (string.Compare(endpoint.Value[i].ConnectionId, connectionId, true) == 0)
						endpoint.Value.RemoveAt(i);
				}
			}
		}

		public void Notify(string endpoint, string arguments)
		{
			var descriptor = ComponentDescriptor.DataHub(new MiddlewareContext(Tenant.Url), endpoint);

			descriptor.Validate();

			var dataHubEndpoint = descriptor.Configuration.Endpoints.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			if (dataHubEndpoint == null)
				throw new RuntimeException($"{SR.ErrDataHubEndpointNotFound} ({endpoint})");

			if (!ConnectionPointers.ContainsKey(dataHubEndpoint.Id))
				return;

			var pointer = ConnectionPointers[dataHubEndpoint.Id];

			foreach (var connection in pointer)
			{
				foreach (var policy in connection.Descriptors)
				{
					var hubPolicy = dataHubEndpoint.Policies.FirstOrDefault(f => string.Compare(f.Name, policy.Key, true) == 0);

					if (hubPolicy == null)
						continue;

					policy.Value.Notify(connection.ConnectionId, arguments);
				}
			}
		}

		private void AttachPointer(Guid endpoint, EndpointConnection connection)
		{
			if (ConnectionPointers.ContainsKey(endpoint))
			{
				ConnectionPointers[endpoint] = new List<EndpointConnection>
				{
					connection
				};
			}
			else
			{
				lock (_connectionPointers)
				{
					if (ConnectionPointers.ContainsKey(endpoint))
					{
						ConnectionPointers[endpoint] = new List<EndpointConnection>
						{
							connection
						};
					}
					else
					{
						ConnectionPointers.TryAdd(endpoint, new List<EndpointConnection>
						{
							connection
						});
					}
				}
			}
		}

		private ConcurrentDictionary<Guid, List<EndpointConnection>> ConnectionPointers => _connectionPointers.Value;
	}
}
