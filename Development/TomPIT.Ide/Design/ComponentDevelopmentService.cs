using Newtonsoft.Json.Linq;
using System;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Design
{
	internal class ComponentDevelopmentService : IComponentDevelopmentService
	{
		public ComponentDevelopmentService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public string CreateName(Guid microService, string category, string prefix)
		{
			var u = Connection.CreateUrl("ComponentDevelopment", "CreateName")
				.AddParameter("microService", microService)
				.AddParameter("category", category)
				.AddParameter("prefix", prefix);

			return Connection.Get<string>(u);
		}

		public void Delete(Guid component)
		{
			var c = Connection.GetService<IComponentService>().SelectComponent(component);

			if (c == null)
				throw new IdeException(SR.ErrComponentNotFound);

			RemoveDependencies(c.Token);

			var u = Connection.CreateUrl("ComponentDevelopment", "Delete");
			var args = new JObject
			{
				{"component", component }
			};

			Connection.Post(u, args);

			if (Connection.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyRemoved(this, new ComponentEventArgs(c.MicroService, c.Feature, component));

			/*
			 * remove configuration file
			 */
			Connection.GetService<IStorageService>().Delete(c.Token);

			if (c.RuntimeConfiguration != Guid.Empty)
				Connection.GetService<IStorageService>().Delete(c.RuntimeConfiguration);

			u = Connection.CreateUrl("NotificationDevelopment", "ConfigurationRemoved");
			args = new JObject
			{
				{ "configuration", c.RuntimeConfiguration},
				{ "microService", c.MicroService},
				{ "category", c.Category}
			};

			Connection.Post(u, args);
		}

		public Guid Insert(IComponent scope, Guid microService, Guid feature, string category, string name, string type)
		{
			var s = Connection.GetService<IMicroServiceService>().Select(microService);

			if (s == null)
				throw new IdeException(SR.ErrMicroServiceNotFound);

			var t = Types.GetType(type);

			if (t == null)
				throw new IdeException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, type));

			var instance = t.CreateInstance<IConfiguration>();

			if (instance == null)
				throw new IdeException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, type));

			instance.Component = Guid.NewGuid();
			instance.ComponentCreated(scope);

			var content = Connection.GetService<ISerializationService>().Serialize(instance);

			var blob = new Blob
			{
				ContentType = "text/xml",
				Draft = Guid.NewGuid(),
				FileName = string.Format("{0}.xml", name),
				ResourceGroup = s.ResourceGroup,
				Size = content.Length,
				MicroService = microService,
				Type = BlobTypes.Configuration,
				Token = instance.Component
			};

			Connection.GetService<IStorageService>().Upload(blob, content, StoragePolicy.Singleton, instance.Component);

			var u = Connection.CreateUrl("ComponentDevelopment", "Insert");
			var args = new JObject
			{
				{"microService", microService },
				{"feature", feature },
				{"name", name },
				{"type", type },
				{"category", category },
				{"component", instance.Component }
			};

			Connection.Post(u, args);
			Connection.GetService<IStorageService>().Commit(blob.Draft, instance.Component.ToString());

			u = Connection.CreateUrl("NotificationDevelopment", "ConfigurationAdded");
			args = new JObject
			{
				{ "configuration", instance.Component }
			};

			Connection.Post(u, args);

			return instance.Component;
		}

		public void Update(Guid component, string name)
		{
			var u = Connection.CreateUrl("ComponentDevelopment", "Update");
			var args = new JObject
			{
				{"name", name },
				{"component", component }
			};

			Connection.Post(u, args);
		}

		public void Update(IConfiguration configuration)
		{
			var c = Connection.GetService<IComponentService>().SelectComponent(configuration.Component);

			if (c == null)
				throw new IdeException(SR.ErrComponentNotFound);

			var s = Connection.GetService<IMicroServiceService>().Select(c.MicroService);

			if (s == null)
				throw new IdeException(SR.ErrMicroServiceNotFound);

			var content = Connection.GetService<ISerializationService>().Serialize(configuration);
			var mode = Shell.GetService<IRuntimeService>().Mode;

			var blob = new Blob
			{
				ContentType = "text/xml",
				FileName = string.Format("{0}.xml", c.Name),
				ResourceGroup = s.ResourceGroup,
				Size = content.Length,
				MicroService = c.MicroService,
				Type = BlobTypes.Configuration,
				PrimaryKey = configuration.Component.ToString()
			};

			if (mode == EnvironmentMode.Runtime)
			{
				var rt = c.RuntimeConfiguration;

				if (rt == Guid.Empty)
				{
					rt = Guid.NewGuid();

					UpdateRuntimeConfiguration(c.Token, rt);
				}

				blob.PrimaryKey = rt.ToString();
				blob.Type = BlobTypes.RuntimeConfiguration;
				blob.Token = rt;

				Connection.GetService<IStorageService>().Upload(blob, content, StoragePolicy.Singleton, rt);
			}
			else
				Connection.GetService<IStorageService>().Upload(blob, content, StoragePolicy.Singleton);


			if (Connection.GetService<IComponentService>() is IComponentNotification n)
				n.NotifyChanged(this, new ConfigurationEventArgs(c.MicroService, configuration.Component, c.Category));

			var u = Connection.CreateUrl("NotificationDevelopment", "ConfigurationChanged");
			var args = new JObject
			{
				{ "configuration", c.Token }
			};

			Connection.Post(u, args);
		}

		private void UpdateRuntimeConfiguration(Guid component, Guid runtimeConfiguration)
		{
			var u = Connection.CreateUrl("ComponentDevelopment", "UpdateRuntimeConfiguration");
			var args = new JObject
			{
				{"runtimeConfiguration", runtimeConfiguration },
				{"component", component }
			};

			Connection.Post(u, args);
		}

		public void Update(IText text, string content)
		{
			if (string.IsNullOrWhiteSpace(content))
				Delete(text);
			else
			{
				var s = Connection.GetService<IMicroServiceService>().Select(text.MicroService(Connection));
				var raw = Encoding.UTF8.GetBytes(content);

				var b = new Blob
				{
					ContentType = "text/xml",
					FileName = string.Format("{0}.xml", text.Id.ToString()),
					PrimaryKey = text.Id.ToString(),
					Size = content.Length,
					MicroService = s.Token,
					ResourceGroup = s.ResourceGroup,
					Type = BlobTypes.Template
				};

				var blob = Connection.GetService<IStorageService>().Upload(b, raw, StoragePolicy.Singleton);

				if (text.TextBlob == Guid.Empty)
				{
					text.TextBlob = blob;
					Update(text.Configuration());
				}
			}
		}

		private void Delete(IText text)
		{
			if (text.TextBlob == Guid.Empty)
				return;

			try
			{
				Connection.GetService<IStorageService>().Delete(text.TextBlob);
			}
			catch { }

			text.TextBlob = Guid.Empty;
			Update(text.Configuration());
		}

		private void RemoveDependencies(Guid component)
		{
			try
			{
				var config = Connection.GetService<IComponentService>().SelectConfiguration(component);

				if (config == null)
					return;

				var txt = config.Children<IText>();

				foreach (var i in txt)
					Delete(i);

				var external = config.Children<IExternalResourceElement>();

				foreach (var i in external)
					i.Delete(new ExternalResourceDeleteArgs(Connection));
			}
			catch (Exception ex)
			{
				Connection.LogWarning(null, "Remove Dependencies", ex.Message);
			}
		}
	}
}
