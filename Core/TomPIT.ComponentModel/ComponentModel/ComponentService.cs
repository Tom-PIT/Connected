using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Design.Serialization;
using TomPIT.Net;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.ComponentModel
{
	internal class ComponentService : ContextCacheRepository<IComponent, Guid>, IComponentService, IComponentNotification
	{
		public event ComponentChangedHandler ComponentChanged;
		public event ConfigurationChangedHandler ConfigurationChanged;
		public event ConfigurationChangedHandler ConfigurationAdded;
		public event ConfigurationChangedHandler ConfigurationRemoved;

		public ComponentService(ISysContext server) : base(server, "component")
		{

		}

		public List<IComponent> QueryComponents(Guid microService, string category)
		{
			var u = Server.CreateUrl("Component", "QueryByCategory")
				.AddParameter("microService", microService)
				.AddParameter("category", category);

			return Server.Connection.Get<List<Component>>(u).ToList<IComponent>();
		}

		public List<IComponent> QueryComponents(Guid microService)
		{
			var u = Server.CreateUrl("Component", "Query")
				.AddParameter("microService", microService);

			return Server.Connection.Get<List<Component>>(u).ToList<IComponent>();
		}

		public IComponent SelectComponent(string category, string name)
		{
			var r = Get(f => string.Compare(f.Category, category, true) == 0
				&& string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			var u = Server.CreateUrl("Component", "SelectByName")
				.AddParameter("category", category)
				.AddParameter("name", name);

			r = Server.Connection.Get<Component>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IComponent SelectComponent(Guid microService, string category, string name)
		{
			var r = Get(f => f.MicroService == microService
				&& string.Compare(f.Category, category, true) == 0
				&& string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			var u = Server.CreateUrl("Component", "Select")
				.AddParameter("microService", microService)
				.AddParameter("category", category)
				.AddParameter("name", name);

			r = Server.Connection.Get<Component>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IComponent SelectComponent(Guid component)
		{
			return Get(component,
				(f) =>
				{
					var u = Server.CreateUrl("Component", "SelectByToken")
						.AddParameter("component", component);

					return Server.Connection.Get<Component>(u);
				});
		}

		public void NotifyChanged(object sender, ComponentEventArgs e)
		{
			Remove(e.Component);
			ComponentChanged?.Invoke(Server, e);
		}

		public string CreateName(Guid microService, string category, string prefix)
		{
			var u = Server.CreateUrl("Component", "CreateName")
				.AddParameter("microService", microService)
				.AddParameter("category", category)
				.AddParameter("prefix", prefix);

			return Server.Connection.Get<string>(u);
		}

		public void NotifyRemoved(object sender, ComponentEventArgs e)
		{
			Remove(e.Component);
		}

		public List<IConfiguration> QueryConfigurations(List<IComponent> components)
		{
			var r = new List<IConfiguration>();
			var ids = components.Select(f => f.Token).Distinct().ToList();
			var rtIds = components.Select(f => f.RuntimeConfiguration).Distinct().Where(f => f != Guid.Empty).ToList();

			var mode = Shell.GetService<IRuntimeService>().Mode;
			var contents = Server.GetService<IStorageService>().Download(ids);
			var runtimeContents = mode == EnvironmentMode.Design ? null : Server.GetService<IStorageService>().Download(rtIds);

			foreach (var i in contents)
			{
				var component = SelectComponent(i.Blob);
				var config = SelectConfiguration(component, i);

				if (mode == EnvironmentMode.Runtime && component.RuntimeConfiguration != Guid.Empty)
				{
					var rt = runtimeContents.FirstOrDefault(f => f.Blob == component.RuntimeConfiguration);

					if (rt != null)
						MergeWithRuntime(config, SelectConfiguration(component, rt));
				}

				r.Add(config);
			}

			return r;
		}

		public List<IConfiguration> QueryConfigurations(List<string> resourceGroups, string categories)
		{
			var r = new List<IConfiguration>();

			var sb = new StringBuilder();

			foreach (var i in resourceGroups)
				sb.AppendFormat("{0},", i.ToString());

			var u = Server.CreateUrl("Component", "QueryByResourceGroups")
				.AddParameter("resourceGroups", sb.ToString().TrimEnd(','))
				.AddParameter("categories", categories);

			return QueryConfigurations(Server.Connection.Get<List<Component>>(u).ToList<IComponent>());
		}

		public IConfiguration SelectConfiguration(Guid microService, string category, string name)
		{
			return SelectConfiguration(SelectComponent(microService, category, name), null);
		}

		public IConfiguration SelectConfiguration(Guid component)
		{
			return SelectConfiguration(SelectComponent(component), null);
		}

		private IConfiguration SelectConfiguration(IComponent component, IBlobContent blob)
		{
			if (component == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			var content = blob ?? Server.GetService<IStorageService>().Download(component.Token);

			if (content == null)
				return null;

			var type = Types.GetType(component.Type);

			if (type == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, component.Type));

			var t = Types.GetType(component.Type);

			var r = Server.GetService<ISerializationService>().Deserialize(content.Content, t) as IConfiguration;

			if (blob == null && Shell.GetService<IRuntimeService>().Mode == EnvironmentMode.Runtime && component.RuntimeConfiguration != Guid.Empty)
			{
				var rtContent = Server.GetService<IStorageService>().Download(component.RuntimeConfiguration);

				if (rtContent != null)
				{
					if (Server.GetService<ISerializationService>().Deserialize(rtContent.Content, t) is IConfiguration rtInstance)
						MergeWithRuntime(r, rtInstance);
				}
			}

			return r;
		}

		public string SelectTemplate(Guid microService, ITemplate template)
		{
			if (template.TemplateBlob == Guid.Empty)
				return null;

			var s = Server.GetService<IMicroServiceService>().Select(microService);

			if (s == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var r = Server.GetService<IStorageService>().Download(template.TemplateBlob);

			if (r == null)
				return null;

			return Encoding.UTF8.GetString(r.Content);
		}

		public void NotifyChanged(object sender, ConfigurationEventArgs e)
		{
			var existing = Get(e.Component);

			if (existing != null)
				Remove(existing.Token);

			ConfigurationChanged?.Invoke(Server, e);
		}

		public void NotifyAdded(object sender, ConfigurationEventArgs e)
		{
			ConfigurationAdded?.Invoke(Server, e);
		}

		public void NotifyRemoved(object sender, ConfigurationEventArgs e)
		{
			ConfigurationRemoved?.Invoke(Server, e);
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

			if (force && att != null && att.Visibility == EnvironmentMode.Design)
				return;

			if (att == null || force || att.Visibility == EnvironmentMode.Runtime)
			{
				var rtValue = runtime.GetType().GetProperty(property.Name).GetValue(runtime);

				property.SetValue(design, rtValue);
			}
		}

		private void MergeCollection(object design, PropertyInfo property, object runtime, List<object> references, bool force)
		{
			var rtProperty = runtime.GetType().GetProperty(property.Name);
			var val = property.GetValue(design);
			var rtVal = rtProperty.GetValue(runtime);

			if (!(val is IEnumerable denum) || !(rtVal is IEnumerable renum))
				return;

			var de = denum.GetEnumerator();
			var re = renum.GetEnumerator();

			while (de.MoveNext())
			{
				re.MoveNext();

				var dinstance = de.Current;

				if (dinstance == null)
					return;

				var rinstance = re.Current;

				if (rinstance == null)
					return;

				var props = dinstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

				foreach (var i in props)
					MergeProperty(dinstance, i, rinstance, references, force);
			}
		}
	}
}
