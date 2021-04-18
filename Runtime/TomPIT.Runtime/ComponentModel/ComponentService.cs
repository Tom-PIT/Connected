using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Development;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Storage;

namespace TomPIT.ComponentModel
{
	internal class ComponentService : SynchronizedClientRepository<IComponent, Guid>, IComponentService, IComponentNotification
	{
		private readonly Lazy<ConcurrentDictionary<Guid, ConfigurationSerializationState>> _configurationCache = new Lazy<ConcurrentDictionary<Guid, ConfigurationSerializationState>>();
		public event ComponentChangedHandler ComponentChanged;
		public event ComponentChangedHandler ComponentAdded;
		public event ComponentChangedHandler ComponentRemoved;
		public event ConfigurationChangedHandler ConfigurationChanged;
		public event ConfigurationChangedHandler ConfigurationAdded;
		public event ConfigurationChangedHandler ConfigurationRemoved;
		public event FolderChangedHandler FolderChanged;

		public ComponentService(ITenant tenant) : base(tenant, "component")
		{
			Tenant.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
			Tenant.GetService<IMicroServiceService>().MicroServiceRemoved += OnMicroServiceRemoved;
			Folders = new FolderCache(Tenant);
		}

		protected override void OnInitialized()
		{
			var resourceGroups = Tenant.GetService<IResourceGroupService>().Query();
			var sb = new StringBuilder();

			foreach (var rg in resourceGroups)
				sb.Append($"{rg.Name},");

			var u = Tenant.CreateUrl("Component", "QueryByResourceGroups");
			var args = new
			{
				resourceGroups = sb.ToString()
			};

			var components = Tenant.Post<List<Component>>(u, args).ToList<IComponent>();

			foreach (var component in components)
				Set(component.Token, component, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Tenant.CreateUrl("Component", "SelectByToken")
				.AddParameter("component", id);

			var component = Tenant.Get<Component>(u);

			if (component != null)
				Set(component.Token, component, TimeSpan.Zero);
		}

		private void OnMicroServiceRemoved(object sender, MicroServiceEventArgs e)
		{
			var components = All();

			foreach (var i in components)
			{
				if (i.MicroService == e.MicroService)
					Remove(i.Token);
			}
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			Folders.RefreshMicroService(e.MicroService);
		}

		private FolderCache Folders { get; }

		public ImmutableList<IComponent> QueryComponents(Guid microService, string category)
		{
			return Where(f => f.MicroService == microService && string.Compare(f.Category, category, true) == 0);
		}

		public ImmutableList<IComponent> QueryComponents(Guid microService, Guid folder)
		{
			return Where(f => f.MicroService == microService && f.Folder == folder);
		}

		public ImmutableList<IComponent> QueryComponents(Guid microService)
		{
			return Where(f => f.MicroService == microService);
		}

		public IComponent SelectComponent(Guid microService, string category, string name)
		{
			return Get(f => f.MicroService == microService
				&& string.Compare(f.Category, category, true) == 0
				&& string.Compare(f.Name, name, true) == 0);
		}

		public IComponent SelectComponentByNameSpace(Guid microService, string nameSpace, string name)
		{
			return Get(f => f.MicroService == microService
				&& string.Compare(f.NameSpace, nameSpace, true) == 0
				&& string.Compare(f.Name, name, true) == 0);
		}

		public IComponent SelectComponent(Guid component)
		{
			return Get(component);
		}

		public void NotifyAdded(object sender, ComponentEventArgs e)
		{
			Refresh(e.Component);
			ComponentAdded?.Invoke(Tenant, e);
		}

		public void NotifyChanged(object sender, ComponentEventArgs e)
		{
			Refresh(e.Component);
			ComponentChanged?.Invoke(Tenant, e);
		}

		public string CreateName(Guid microService, string category, string prefix)
		{
			var u = Tenant.CreateUrl("Component", "CreateName")
				.AddParameter("microService", microService)
				.AddParameter("category", category)
				.AddParameter("prefix", prefix);

			return Tenant.Get<string>(u);
		}

		public void NotifyRemoved(object sender, ComponentEventArgs e)
		{
			Remove(e.Component);
			ComponentRemoved?.Invoke(Tenant, e);
		}

		public ImmutableList<IConfiguration> QueryConfigurations(ImmutableList<IComponent> components)
		{
			var r = new List<IConfiguration>();
			var ids = components.Select(f => f.Token).Distinct().ToList();
			var contents = Tenant.GetService<IStorageService>().Download(ids);

			foreach(var content in contents)
			{
				var component = components.FirstOrDefault(f => f.Token == content.Blob);

				if (component == null)
					continue;

				var config = SelectConfiguration(component, content, false);

				r.Add(config);
			};

			return r.ToImmutableList();
		}

		public ImmutableList<IConfiguration> QueryConfigurations(string category)
		{
			return QueryConfigurations(Where(f => string.Compare(f.Category, category, true) == 0));
		}

		public ImmutableList<IConfiguration> QueryConfigurations(List<Guid> microServices, string category)
		{
			return QueryConfigurations(Where(f => microServices.Any(g => g == f.Token) && string.Compare(f.Category, category, true) == 0));
		}
		public ImmutableList<IConfiguration> QueryConfigurations(Guid microService, string categories)
		{
			var cats = categories.Split(',');
			var r = new List<IComponent>();

			foreach (var j in cats)
			{
				if (string.IsNullOrWhiteSpace(j))
					continue;

				var ds = QueryComponents(microService, j.Trim());

				if (ds.Count > 0)
					r.AddRange(ds);
			}

			return QueryConfigurations(r.ToImmutableList());
		}

		public ImmutableList<IConfiguration> QueryConfigurations(List<string> resourceGroups, string categories)
		{
			var cats = string.IsNullOrWhiteSpace(categories) ? Array.Empty<string>() : categories.Split(',', StringSplitOptions.RemoveEmptyEntries);
			var r = new List<IComponent>();
			var microServices = new List<IMicroService>();

			if (resourceGroups.Count == 0)
			{
				var ms = Tenant.GetService<IMicroServiceService>().Query();

				if (ms != null && ms.Count > 0)
					microServices.AddRange(ms);
			}
			else
			{
				foreach (var i in resourceGroups)
				{
					var rs = Tenant.GetService<IResourceGroupService>().Select(i);

					if (rs == null)
						throw new RuntimeException($"{SR.ErrResourceGroupNotFound} ({i})");

					var sols = Tenant.GetService<IMicroServiceService>().Query().Where(f=>f.ResourceGroup==rs.Token).ToList();

					if (sols.Count > 0)
						microServices.AddRange(sols);
				}
			}

			foreach (var i in microServices)
			{
				if (cats.Length == 0)
				{
					var ds = QueryComponents(i.Token);

					if (ds.Count > 0)
						r.AddRange(ds);
				}
				else
				{
					foreach (var j in cats)
					{
						if (string.IsNullOrWhiteSpace(j))
							continue;

						var ds = QueryComponents(i.Token, j.Trim());

						if (ds.Count > 0)
							r.AddRange(ds);
					}
				}
			}

			return QueryConfigurations(r.Where(f => f.LockVerb != LockVerb.Delete).ToImmutableList());
		}

		public IConfiguration SelectConfiguration(Guid microService, string category, string name)
		{
			var cmp = SelectComponent(microService, category, name);

			if (cmp == null)
				return null;

			return SelectConfiguration(cmp, null, true);
		}

		public IConfiguration SelectConfiguration(Guid component)
		{
			var cmp = SelectComponent(component);

			if (cmp == null)
				return null;

			return SelectConfiguration(cmp, null, true);
		}

		private IConfiguration SelectConfiguration(IComponent component, IBlobContent blob, bool throwException)
		{
			if (component == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			if (ConfigurationCache.ContainsKey(component.Token))
			{
				var state = ConfigurationCache[component.Token];

				return state.Instance;
				//return Tenant.GetService<ISerializationService>().Deserialize(state.State, state.Type) as IConfiguration;
			}

			var content = blob ?? Tenant.GetService<IStorageService>().Download(component.Token);

			if (content == null)
				return null;

			var type = Reflection.TypeExtensions.GetType(component.Type);

			if (type == null)
				return throwException ? throw new RuntimeException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, component.Type)) : (IConfiguration)null;

			var t = Reflection.TypeExtensions.GetType(component.Type);
			IConfiguration r = null;

			try
			{
				r = Tenant.GetService<ISerializationService>().Deserialize(content.Content, t) as IConfiguration;
			}
			catch (Exception ex)
			{
				if (throwException)
					throw;
				else
					Tenant.LogError(GetType().ShortName(), ex.Message, LogCategories.Services);
			}

			if (r != null)
			{
				ConfigurationCache.TryAdd(component.Token, new ConfigurationSerializationState
				{
					Type = r.GetType(),
					Instance = r,
					State = Tenant.GetService<ISerializationService>().Serialize(r)
				});
			}

			return r;
		}

		public string SelectText(Guid microService, IText text)
		{
			if (text.TextBlob == Guid.Empty)
				return null;

			var s = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (s == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var r = Tenant.GetService<IStorageService>().Download(text.TextBlob);

			if (r == null)
				return null;

			return Encoding.UTF8.GetString(r.Content);
		}

		public void NotifyChanged(object sender, ConfigurationEventArgs e)
		{
			var existing = Get(e.Component);

			if (existing != null)
				Refresh(e.Component);

			ConfigurationCache.Remove(e.Component, out _);
			ConfigurationChanged?.Invoke(Tenant, e);
		}

		public void NotifyAdded(object sender, ConfigurationEventArgs e)
		{
			ConfigurationAdded?.Invoke(Tenant, e);
		}

		public void NotifyRemoved(object sender, ConfigurationEventArgs e)
		{
			ConfigurationRemoved?.Invoke(Tenant, e);
		}

		public IFolder SelectFolder(Guid folder)
		{
			return Folders.Select(folder);
		}

		public ImmutableList<IFolder> QueryFolders(Guid microService, Guid parent)
		{
			return Folders.Query(microService, parent);
		}

		public ImmutableList<IFolder> QueryFolders(Guid microService)
		{
			return Folders.Query(microService);
		}

		public void NotifyFolderChanged(object sender, FolderEventArgs e)
		{
			Folders.Reload(e.Folder);
			FolderChanged?.Invoke(sender, e);
		}

		public void NotifyFolderRemoved(object sender, FolderEventArgs e)
		{
			Folders.Delete(e.Folder);
			FolderChanged?.Invoke(sender, e);
		}

		private ConcurrentDictionary<Guid, ConfigurationSerializationState> ConfigurationCache => _configurationCache.Value;

		public ImmutableList<IComponent> QueryComponents(List<string> resourceGroups, string categories)
		{
			var sb = new StringBuilder();

			foreach (var i in resourceGroups)
				sb.AppendFormat("{0},", i.ToString());

			var u = Tenant.CreateUrl("Component", "QueryByResourceGroups");

			return Tenant.Post<ImmutableList<Component>>(u, new
			{
				resourceGroups = sb.ToString().TrimEnd(','),
				categories
			}).ToImmutableList<IComponent>();
		}
	}
}
