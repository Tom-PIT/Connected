using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Annotations.Design;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.ComponentModel
{
	internal class ComponentService : ClientRepository<IComponent, Guid>, IComponentService, IComponentNotification
	{
		private Lazy<ConcurrentDictionary<Guid, ConfigurationSerializationState>> _configurationCache = new Lazy<ConcurrentDictionary<Guid, ConfigurationSerializationState>>();
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
			Folders = new FolderCache(Tenant);
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			var components = All();

			foreach (var i in components)
			{
				if (i.MicroService == e.MicroService)
					Remove(i.Token);
			}

			Folders.RefreshMicroService(e.MicroService);
		}

		private FolderCache Folders { get; }

		public List<IComponent> QueryComponents(Guid microService, string category)
		{
			var u = Tenant.CreateUrl("Component", "QueryByCategory")
				.AddParameter("microService", microService)
				.AddParameter("category", category);

			return CacheComponents(Tenant.Get<List<Component>>(u).ToList<IComponent>());
		}

		public List<IComponent> QueryComponents(Guid microService, Guid folder)
		{
			var u = Tenant.CreateUrl("Component", "QueryByFolder")
				.AddParameter("microService", microService)
				.AddParameter("folder", folder);

			return CacheComponents(Tenant.Get<List<Component>>(u).ToList<IComponent>());
		}

		public List<IComponent> QueryComponents(Guid microService)
		{
			var u = Tenant.CreateUrl("Component", "Query")
				.AddParameter("microService", microService);

			return CacheComponents(Tenant.Get<List<Component>>(u).ToList<IComponent>());
		}

		public IComponent SelectComponent(Guid microService, string category, string name)
		{
			var r = Get(f => f.MicroService == microService
				&& string.Compare(f.Category, category, true) == 0
				&& string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("Component", "Select")
				.AddParameter("microService", microService)
				.AddParameter("category", category)
				.AddParameter("name", name);

			r = Tenant.Get<Component>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IComponent SelectComponentByNameSpace(Guid microService, string nameSpace, string name)
		{
			var r = Get(f => f.MicroService == microService
				&& string.Compare(f.NameSpace, nameSpace, true) == 0
				&& string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("Component", "SelectByNameSpace")
				.AddParameter("microService", microService)
				.AddParameter("nameSpace", nameSpace)
				.AddParameter("name", name);

			r = Tenant.Get<Component>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IComponent SelectComponent(Guid component)
		{
			return Get(component,
				(f) =>
				{
					var u = Tenant.CreateUrl("Component", "SelectByToken")
						.AddParameter("component", component);

					return Tenant.Get<Component>(u);
				});
		}

		public void NotifyAdded(object sender, ComponentEventArgs e)
		{
			ComponentAdded?.Invoke(Tenant, e);
		}

		public void NotifyChanged(object sender, ComponentEventArgs e)
		{
			Remove(e.Component);
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

		public List<IConfiguration> QueryConfigurations(List<IComponent> components)
		{
			var r = new List<IConfiguration>();
			var ids = components.Select(f => f.Token).Distinct().ToList();
			var rtIds = components.Select(f => f.RuntimeConfiguration).Distinct().Where(f => f != Guid.Empty).ToList();

			var mode = Shell.GetService<IRuntimeService>().Mode;
			var contents = Tenant.GetService<IStorageService>().Download(ids);
			var runtimeContents = mode == EnvironmentMode.Design ? null : Tenant.GetService<IStorageService>().Download(rtIds);

			Parallel.ForEach(contents, (i) =>
			{
				var component = SelectComponent(i.Blob);

				if (component == null)
					return;

				IBlobContent runtime = null;

				if (mode == EnvironmentMode.Runtime && component.RuntimeConfiguration != Guid.Empty)
					runtime = runtimeContents.FirstOrDefault(f => f.Blob == component.RuntimeConfiguration);

				var config = SelectConfiguration(component, i, runtime, false);

				r.Add(config);
			});

			return r;
		}

		public List<IConfiguration> QueryConfigurations(Guid microService, string categories)
		{
			var r = new List<IConfiguration>();

			var sb = new StringBuilder();

			var u = Tenant.CreateUrl("Component", "QueryByMicroService")
				.AddParameter("microService", microService)
				.AddParameter("categories", categories);

			return QueryConfigurations(Tenant.Get<List<Component>>(u).ToList<IComponent>());
		}

		public List<IConfiguration> QueryConfigurations(List<string> resourceGroups, string categories)
		{
			var r = new List<IConfiguration>();

			var sb = new StringBuilder();

			foreach (var i in resourceGroups)
				sb.AppendFormat("{0},", i.ToString());

			var u = Tenant.CreateUrl("Component", "QueryByResourceGroups")
				.AddParameter("resourceGroups", sb.ToString().TrimEnd(','))
				.AddParameter("categories", categories);

			return QueryConfigurations(Tenant.Get<List<Component>>(u).ToList<IComponent>());
		}

		public IConfiguration SelectConfiguration(Guid microService, string category, string name)
		{
			var cmp = SelectComponent(microService, category, name);

			if (cmp == null)
				return null;

			return SelectConfiguration(cmp, null, null, true);
		}

		public IConfiguration SelectConfiguration(Guid component)
		{
			var cmp = SelectComponent(component);

			if (cmp == null)
				return null;

			return SelectConfiguration(cmp, null, null, true);
		}

		private IConfiguration SelectConfiguration(IComponent component, IBlobContent blob, IBlobContent runtime, bool throwException)
		{
			if (component == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			if (ConfigurationCache.ContainsKey(component.Token))
			{
				var state = ConfigurationCache[component.Token];

				return Tenant.GetService<ISerializationService>().Deserialize(state.State, state.Type) as IConfiguration;
			}

			var content = blob ?? Tenant.GetService<IStorageService>().Download(component.Token);

			if (content == null)
				return null;

			var type = Reflection.TypeExtensions.GetType(component.Type);

			if (type == null)
			{
				if (throwException)
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, component.Type));
				else
					return null;
			}

			var t = Reflection.TypeExtensions.GetType(component.Type);
			IConfiguration r = null;

			try
			{
				r = Tenant.GetService<ISerializationService>().Deserialize(content.Content, t) as IConfiguration;
			}
			catch (Exception ex)
			{
				if (throwException)
					throw ex;
				else
					Tenant.LogError(GetType().ShortName(), ex.Message, LogCategories.Services);
			}

			if (blob == null && Shell.GetService<IRuntimeService>().Mode == EnvironmentMode.Runtime && component.RuntimeConfiguration != Guid.Empty)
			{
				var rtContent = runtime == null
					? Tenant.GetService<IStorageService>().Download(component.RuntimeConfiguration)
					: runtime;

				if (rtContent != null)
				{
					try
					{
						if (Tenant.GetService<ISerializationService>().Deserialize(rtContent.Content, t) is IConfiguration rtInstance)
							MergeWithRuntime(r, rtInstance);
					}
					catch (Exception ex)
					{
						Tenant.LogWarning(GetType().ShortName(), ex.Message, LogCategories.Services);
					}
				}
			}

			ConfigurationCache.TryAdd(component.Token, new ConfigurationSerializationState
			{
				Type = r.GetType(),
				State = Tenant.GetService<ISerializationService>().Serialize(r)
			});

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
				Remove(existing.Token);

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

		private void MergeWithRuntime(IConfiguration design, IConfiguration runtime)
		{
			if (design == null || runtime == null)
				return;

			MergeProperties(design, runtime, new List<object>(), false);
		}

		private void MergeProperties(object design, object runtime, List<object> references, bool force)
		{
			var props = design.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var i in props)
				MergeProperty(design, i, runtime, references, force);
		}

		private void MergeProperty(object design, PropertyInfo property, object runtime, List<object> references, bool force)
		{
			if (design == null)
				return;

			if (runtime == null)
				return;

			if (property.IsPrimitive())
				SetProperty(design, property, runtime, references, force);
			else if (property.PropertyType.IsCollection())
				MergeCollection(design, property, runtime, references, force);
			else
				MergeObject(design, property, runtime, references, force);
		}

		private void MergeObject(object design, PropertyInfo property, object runtime, List<object> references, bool force)
		{
			if (property.IsIndexer())
				return;

			var value = property.GetValue(design);

			if (value == null || runtime == null)
				return;

			if (!property.IsPrimitive())
			{
				if (references.Contains(value))
					return;

				references.Add(value);
			}

			var att = property.FindAttribute<EnvironmentVisibilityAttribute>();

			if (att != null)
			{
				if (att.Visibility == EnvironmentMode.Runtime)
					force = true;
				else
					force = false;
			}

			var refValue = runtime.GetType().GetProperty(property.Name).GetValue(runtime);

			if (refValue == null)
				return;

			MergeProperties(value, refValue, references, force);
		}

		private void SetProperty(object design, PropertyInfo property, object runtime, List<object> references, bool force)
		{
			if (!property.CanWrite)
				return;

			var att = property.FindAttribute<EnvironmentVisibilityAttribute>();

			if (att == null && !force)
				return;

			if (force && att != null && ((att.Visibility & EnvironmentMode.Design) == EnvironmentMode.Design))
				return;

			if (att == null || force || ((att.Visibility & EnvironmentMode.Runtime) == EnvironmentMode.Runtime))
			{
				var rtValue = runtime.GetType().GetProperty(property.Name).GetValue(runtime);

				property.SetValue(design, rtValue);
			}
		}

		private void MergeCollection(object design, PropertyInfo property, object runtime, List<object> references, bool force)
		{
			CollectionRuntimeMerge mode = CollectionRuntimeMerge.Synchronize;

			var att = property.FindAttribute<CollectionRuntimeMergeAttribute>();

			if (att != null)
				mode = att.Mode;

			switch (mode)
			{
				case CollectionRuntimeMerge.Synchronize:
					MergeCollectionSynchronize(design, property, runtime, references, force);
					break;
				case CollectionRuntimeMerge.Override:
					MergeCollectionOverride(design, property, runtime, references, force);
					break;
				case CollectionRuntimeMerge.Append:
					MergeCollectionAppend(design, property, runtime, references, force);
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private void MergeCollectionSynchronize(object design, PropertyInfo property, object runtime, List<object> references, bool force)
		{
			var rtProperty = runtime.GetType().GetProperty(property.Name);
			var val = property.GetValue(design);
			var rtVal = rtProperty.GetValue(runtime);

			if (val is IEnumerable<IElement> dEltEnum && rtVal is IEnumerable<IElement> rEltEnum)
			{
				MergeCollectionSynchronizeElements(dEltEnum, rEltEnum, references, force);
				return;
			}

			if (!(val is IEnumerable denum) || !(rtVal is IEnumerable renum))
			{
				return;
			}

			// Note: the code below assumes that lists contain items with the same
			//			order. This might not be the case!!!
			var de = denum.GetEnumerator();
			var re = renum.GetEnumerator();

			while (de.MoveNext())
			{
				if (!re.MoveNext())
				{
					break;
				}

				var dinstance = de.Current;
				if (dinstance == null)
				{
					return;
				}

				var rinstance = re.Current;
				if (rinstance == null)
				{
					return;
				}

				var props = dinstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
				foreach (var i in props)
				{
					MergeProperty(dinstance, i, rinstance, references, force);
				}
			}
		}

		private void MergeCollectionSynchronizeElements(IEnumerable<IElement> dEltEnum, IEnumerable<IElement> rEltEnum, List<object> references, bool force)
		{
			var de = dEltEnum.GetEnumerator();
			while (de.MoveNext())
			{
				var dinstance = de.Current;
				if (dinstance == null)
				{
					return;
				}

				var rinstance = rEltEnum.FirstOrDefault(x => x.Id == dinstance.Id);
				if (rinstance == null)
				{
					continue;
				}

				var props = dinstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
				foreach (var i in props)
				{
					MergeProperty(dinstance, i, rinstance, references, force);
				}
			}
		}

		private void MergeCollectionAppend(object design, PropertyInfo property, object runtime, List<object> references, bool force)
		{
			var rtProperty = runtime.GetType().GetProperty(property.Name);
			var val = property.GetValue(design);
			var rtVal = rtProperty.GetValue(runtime);

			if (!(val is IEnumerable denum) || !(rtVal is IEnumerable renum))
				return;

			var de = denum.GetEnumerator();
			var re = renum.GetEnumerator();

			while (re.MoveNext())
			{
				var rinstance = re.Current;

				if (rinstance == null)
					return;

				var method = val.GetType().GetRuntimeMethod("Add", new Type[] { rinstance.GetType() });

				if (method == null)
					return;

				method.Invoke(val, new object[] { rinstance });
			}
		}

		private void MergeCollectionOverride(object design, PropertyInfo property, object runtime, List<object> references, bool force)
		{
			var val = property.GetValue(design);

			if (val == null)
				return;

			var clear = val.GetType().GetRuntimeMethod("Clear", new Type[0]);

			if (clear != null)
				clear.Invoke(val, null);

			MergeCollectionAppend(design, property, runtime, references, force);
		}

		public IFolder SelectFolder(Guid folder)
		{
			return Folders.Select(folder);
		}

		public List<IFolder> QueryFolders(Guid microService, Guid parent)
		{
			return Folders.Query(microService, parent);
		}

		public List<IFolder> QueryFolders(Guid microService)
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

		private List<IComponent> CacheComponents(List<IComponent> components)
		{
			if (components == null)
				return components;

			foreach (var component in components)
				Set(component.Token, component);

			return components;
		}
	}
}
