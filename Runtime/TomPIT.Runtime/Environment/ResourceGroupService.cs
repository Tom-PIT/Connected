using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Runtime;

namespace TomPIT.Environment
{
	internal class ResourceGroupService : ClientRepository<IResourceGroup, Guid>, IResourceGroupService
	{
		public ResourceGroupService(ITenant tenant) : base(tenant, "resourceGroup")
		{
			Supported = new();

			Initialize();
		}

		private List<IResourceGroup> Supported { get; }
		public IResourceGroup Default => Select(new Guid("E14372D117CD48D6BC29D57C397AF87C"));

		public ImmutableList<IResourceGroup> QuerySupported()
		{
			return Supported.ToImmutableList();
		}
		public List<IResourceGroup> Query()
		{
			return Instance.SysProxy.ResourceGroups.Query().ToList();
		}

		public IResourceGroup Select(string name)
		{
			var r = Get(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

			if (r is not null)
				return r;

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
			{
				if (Supported.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)) is null)
					return null;
			}

			r = Instance.SysProxy.ResourceGroups.Select(name);

			if (r is not null)
				Set(r.Token, r, TimeSpan.Zero);

			return r;
		}

		public IResourceGroup Select(Guid resourceGroup)
		{
			return Get(resourceGroup,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var r = Instance.SysProxy.ResourceGroups.Select(resourceGroup);

					if (r is not null)
					{
						if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
						{
							if (Supported.FirstOrDefault(f => string.Equals(f.Name, r.Name, StringComparison.OrdinalIgnoreCase)) is null)
								return null;
						}
					}

					return r;
				});
		}

		private void Initialize()
		{
			if (!Shell.Configuration.RootElement.TryGetProperty("resourceGroups", out JsonElement element))
				return;

			var all = Query();

			foreach (var item in element.EnumerateArray())
			{
				var current = all.FirstOrDefault(f => string.Equals(f.Name, item.GetString(), StringComparison.OrdinalIgnoreCase));

				if (current is null)
					continue;

				if (Supported.Any(f => f.Token == current.Token))
					continue;

				Supported.Add(current);
			}
		}
	}
}
