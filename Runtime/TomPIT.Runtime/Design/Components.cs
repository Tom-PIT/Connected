using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design.Serialization;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Storage;

namespace TomPIT.Design
{
	internal class Components : TenantObject, IComponentModel
	{
		public Components(ITenant tenant) : base(tenant)
		{
		}

		public string CreateName(Guid microService, string category, string prefix)
		{
			var u = Tenant.CreateUrl("ComponentDevelopment", "CreateName")
				 .AddParameter("microService", microService)
				 .AddParameter("nameSpace", ComponentCategories.ResolveNamespace(category))
				 .AddParameter("prefix", prefix);

			return Tenant.Get<string>(u);
		}

		public void Delete(Guid component)
		{
			Delete(component, false);
		}

		public void Delete(Guid component, bool permanent)
		{
			var c = Tenant.GetService<IComponentService>().SelectComponent(component);

			if (c == null)
				return;

			if (!permanent)
				Tenant.GetService<IDesignService>().VersionControl.Lock(component, Development.LockVerb.Delete);

			if (permanent)
			{
				var config = Tenant.GetService<IComponentService>().SelectConfiguration(c.Token);

				if (config != null)
				{
					var texts = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

					foreach (var text in texts)
						Delete(text, false);
				}

				RemoveDependencies(c.Token);

				Tenant.GetService<IDesignService>().VersionControl.DeleteHistory(component);
			}

			var u = Tenant.CreateUrl("ComponentDevelopment", "Delete");
			var args = new JObject
				{
					 {"component", component },
					 {"permanent", permanent },
					 {"user", MiddlewareDescriptor.Current.UserToken }
				};

			Tenant.Post(u, args);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyRemoved(this, new ComponentEventArgs(c.MicroService, c.Folder, component, c.NameSpace, c.Category, c.Name));

			/*
		 * remove configuration file
		 */
			if (permanent)
			{
				Tenant.GetService<IStorageService>().Delete(c.Token);

				//if (c.RuntimeConfiguration != Guid.Empty)
				//	Tenant.GetService<IStorageService>().Delete(c.RuntimeConfiguration);

				Tenant.GetService<IDesignService>().Search.Delete(c.Token);
			}

			//DeleteManifest(c);

			u = Tenant.CreateUrl("NotificationDevelopment", "ConfigurationRemoved");
			args = new JObject
				{
					 { "configuration", c.Token},
					 { "microService", c.MicroService},
					 { "category", c.Category}
				};

			Tenant.Post(u, args);
		}

		public void Restore(Guid microService, IPackageComponent component, IPackageBlob configuration)
		{
			var runtimeConfigurationId = component.RuntimeConfiguration;
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			var blob = new Blob
			{
				ContentType = configuration.ContentType,
				FileName = configuration.FileName,
				ResourceGroup = ms.ResourceGroup,
				MicroService = microService,
				Type = configuration.Type,
				Token = configuration.Token,
				PrimaryKey = configuration.PrimaryKey
			};

			Tenant.GetService<IStorageService>().Upload(blob, Convert.FromBase64String(configuration.Content), StoragePolicy.Singleton, component.Token);

			var u = Tenant.CreateUrl("ComponentDevelopment", "Insert");

			var args = new JObject
				{
					 {"microService", microService },
					 {"folder", component.Folder },
					 {"name", component.Name },
					 {"type", component.Type },
					 {"category", component.Category },
					 {"component", component.Token },
					 {"nameSpace", ComponentCategories.ResolveNamespace( component.Category) }
				};

			if (runtimeConfigurationId != Guid.Empty)
				args.Add("runtimeConfiguration", runtimeConfigurationId);

			Tenant.Post(u, args);

			if (Tenant.GetService<IComponentService>() is IComponentNotification notification)
			{
				notification.NotifyChanged(this, new ComponentEventArgs
				{
					Category = component.Category,
					Component = component.Token,
					Folder = component.Folder,
					MicroService = microService,
					Name = component.Name,
					NameSpace = ComponentCategories.ResolveNamespace(component.Category)
				});
			}

			InvalidateIndexState(component.Token);
		}

		public void Restore(Guid microService, IPullRequestComponent component)
		{
			if (component.Verb == ComponentVerb.Delete)
			{
				Delete(component.Token, true);
				NotifyRemoted(microService, component);

				return;
			}

			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);
			var configuration = component.Files.FirstOrDefault(f => f.Type == BlobTypes.Configuration);

			if (configuration is not null && (configuration.Verb == ComponentVerb.Add || configuration.Verb == ComponentVerb.Edit))
			{
				var blob = new Blob
				{
					ContentType = configuration.ContentType,
					FileName = configuration.FileName,
					ResourceGroup = ms.ResourceGroup,
					MicroService = microService,
					Type = configuration.Type,
					Token = component.Token,
					PrimaryKey = component.Token.ToString()
				};

				Tenant.GetService<IStorageService>().Upload(blob, Unpack(configuration.Content), StoragePolicy.Singleton, component.Token);
			}

			foreach (var file in component.Files)
			{
				if (file.Type == BlobTypes.Configuration || file.Type == BlobTypes.RuntimeConfiguration)
					continue;

				if (file.Verb == ComponentVerb.NotModified)
					continue;
				else if (file.Verb == ComponentVerb.Delete)
					Tenant.GetService<IStorageService>().Delete(file.Token);
				else
				{
					Tenant.GetService<IStorageService>().Restore(new Blob
					{
						ContentType = file.ContentType,
						FileName = file.FileName,
						MicroService = microService,
						ResourceGroup = ms.ResourceGroup,
						Token = file.Token,
						PrimaryKey = file.PrimaryKey,
						Topic = file.Topic,
						Type = file.Type,
						Version = file.BlobVersion
					}, Unpack(file.Content));
				}
			}

			if (component.Verb == ComponentVerb.Add)
			{
				var u = Tenant.CreateUrl("ComponentDevelopment", "Insert");

				var args = new JObject
				{
					 {"microService", microService },
					 {"folder", component.Folder },
					 {"name", component.Name },
					 {"type", component.Type },
					 {"category", component.Category },
					 {"component", component.Token },
					 {"nameSpace", ComponentCategories.ResolveNamespace( component.Category) }
				};

				Tenant.Post(u, args);

				if (Tenant.GetService<IComponentService>() is IComponentNotification notification)
				{
					notification.NotifyChanged(this, new ComponentEventArgs
					{
						Category = component.Category,
						Component = component.Token,
						Folder = component.Folder,
						MicroService = microService,
						Name = component.Name,
						NameSpace = ComponentCategories.ResolveNamespace(component.Category)
					});

					notification.NotifyChanged(this, new ConfigurationEventArgs
					{
						Category = component.Category,
						Component = component.Token,
						MicroService = microService
					});
				}
			}
			else
				Update(component.Token, component.Name, component.Folder, false);

			InvalidateIndexState(component.Token);
		}

		private void NotifyRemoted(Guid microService, IPullRequestComponent component)
		{
			if (Tenant.GetService<IComponentService>() is IComponentNotification notification)
			{
				notification.NotifyRemoved(this, new ComponentEventArgs
				{
					Category = component.Category,
					Component = component.Token,
					Folder = component.Folder,
					MicroService = microService,
					Name = component.Name,
					NameSpace = ComponentCategories.ResolveNamespace(component.Category)
				});
			}
		}

		private static byte[] Unpack(string packed)
		{
			using var input = new MemoryStream(Convert.FromBase64String(packed));
			using var zip = new GZipStream(input, CompressionMode.Decompress);
			using var output = new MemoryStream();

			zip.CopyTo(output);

			return output.ToArray();
		}

		public Guid Clone(Guid component, Guid microService, Guid folder)
		{
			var existing = Tenant.GetService<IComponentService>().SelectComponent(component);

			if (existing == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			var ds = Tenant.GetService<IDiscoveryService>();

			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);
			var existingConfiguration = Tenant.GetService<IComponentService>().SelectConfiguration(component);
			var blobs = ds.Configuration.Query<IText>(existingConfiguration);
			var elements = ds.Configuration.Query<IElement>(existingConfiguration);
			var externals = ds.Configuration.Query<IExternalResourceElement>(existingConfiguration);
			var newId = Insert(microService, folder, existing.Category, CreateName(microService, existing.Category, existing.Name), existing.Type);

			foreach (var element in elements)
				element.Reset();

			existingConfiguration.Component = newId;

			foreach (var blob in blobs)
			{
				if (blob.TextBlob == Guid.Empty)
					continue;

				var content = Tenant.GetService<IStorageService>().Download(blob.TextBlob);
				var text = content == null || content.Content == null || content.Content.Length == 0 ? string.Empty : Encoding.UTF8.GetString(content.Content);

				blob.TextBlob = Guid.Empty;

				Update(blob, text);
			}

			foreach (var external in externals)
			{
				var resources = external.QueryResources();

				if (resources == null || resources.Count == 0)
					continue;

				foreach (var resource in resources)
				{
					var resourceBlob = Tenant.GetService<IStorageService>().Select(resource);

					if (resourceBlob == null)
						continue;

					var resourceBlobContent = Tenant.GetService<IStorageService>().Download(resourceBlob.Token);
					var newBlob = new Blob
					{
						ContentType = resourceBlob.ContentType,
						FileName = resourceBlob.FileName,
						MicroService = microService,
						PrimaryKey = external.Id.ToString(),
						ResourceGroup = ms.ResourceGroup,
						Type = resourceBlob.Type,
						Topic = resourceBlob.Topic
					};

					var token = Tenant.GetService<IStorageService>().Upload(newBlob, resourceBlobContent.Content, StoragePolicy.Singleton);

					external.Reset(resource, token);
				}
			}

			Update(existingConfiguration);

			return newId;
		}
		public Guid Insert(Guid microService, Guid folder, string category, string name, string type)
		{
			var s = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (s == null)
				throw new NotFoundException(SR.ErrMicroServiceNotFound);

			var t = Reflection.TypeExtensions.GetType(type);

			if (t == null)
				throw new TomPITException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, type));

			var instance = t.CreateInstance<IConfiguration>();

			if (instance == null)
				throw new TomPITException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, type));

			instance.Component = Guid.NewGuid();
			instance.ComponentCreated();

			var content = Tenant.GetService<ISerializationService>().Serialize(instance);

			var blob = new Blob
			{
				ContentType = "application/json",
				Draft = Guid.NewGuid().ToString(),
				FileName = string.Format("{0}.json", name),
				ResourceGroup = s.ResourceGroup,
				Size = content.Length,
				MicroService = microService,
				Type = BlobTypes.Configuration,
				Token = instance.Component
			};

			Tenant.GetService<IStorageService>().Upload(blob, content, StoragePolicy.Singleton, instance.Component);

			var u = Tenant.CreateUrl("ComponentDevelopment", "Insert");
			var args = new JObject
				{
					 {"microService", microService },
					 {"folder", folder },
					 {"name", name },
					 {"type", type },
					 {"category", category },
					 {"component", instance.Component },
					 {"nameSpace", ComponentCategories.ResolveNamespace(category) }
				};

			Tenant.Post(u, args);
			Tenant.GetService<IStorageService>().Commit(blob.Draft, instance.Component.ToString());

			if (Tenant.GetService<IComponentService>() is IComponentNotification notification)
			{
				notification.NotifyAdded(this, new ComponentEventArgs
				{
					Category = category,
					Folder = folder,
					Component = instance.Component,
					MicroService = microService,
					Name = name,
					NameSpace = ComponentCategories.ResolveNamespace(category)
				});
			}

			InvalidateIndexState(instance.Component);

			u = Tenant.CreateUrl("NotificationDevelopment", "ConfigurationAdded");
			args = new JObject
				{
					 { "configuration", instance.Component }
				};

			Tenant.Post(u, args);
			Tenant.GetService<IDesignService>().VersionControl.Lock(instance.Component, Development.LockVerb.Add);

			return instance.Component;
		}

		public void Update(Guid component, string name, Guid folder, bool performLock)
		{
			if (performLock)
				Tenant.GetService<IDesignService>().VersionControl.Lock(component, Development.LockVerb.Edit);

			//DeleteManifest(component);

			var c = Tenant.GetService<IComponentService>().SelectComponent(component);

			if (c == null)
				throw new TomPITException(SR.ErrComponentNotFound);

			Tenant.Post(CreateUrl("Update"), new
			{
				name,
				component,
				folder
			});

			InvalidateIndexState(component);

			if (Tenant.GetService<IComponentService>() is IComponentNotification n)
				n.NotifyChanged(this, new ConfigurationEventArgs(c.MicroService, component, c.Category));
		}

		public void Update(Guid component, string name, Guid folder)
		{
			Update(component, name, folder, true);
		}

		public void Update(IConfiguration configuration)
		{
			UpdateConfiguration(configuration, new ComponentUpdateArgs(true));
		}

		public void Update(IConfiguration configuration, ComponentUpdateArgs e)
		{
			UpdateConfiguration(configuration, e);
		}

		private void UpdateConfiguration(IConfiguration configuration, ComponentUpdateArgs e)
		{
			var c = Tenant.GetService<IComponentService>().SelectComponent(configuration.Component);

			if (c == null)
				throw new TomPITException(SR.ErrComponentNotFound);

			var s = Tenant.GetService<IMicroServiceService>().Select(c.MicroService);

			if (s == null)
				throw new TomPITException(SR.ErrMicroServiceNotFound);
			/*
		 * version control lock needs to be obtained only for design time
		 */
			if (e.PerformLock)
				Tenant.GetService<IDesignService>().VersionControl.Lock(c.Token, Development.LockVerb.Edit);

			var content = Tenant.GetService<ISerializationService>().Serialize(configuration);

			var blob = new Blob
			{
				ContentType = "application/json",
				FileName = string.Format("{0}.json", c.Name),
				ResourceGroup = s.ResourceGroup,
				Size = content.Length,
				MicroService = c.MicroService,
				Type = BlobTypes.Configuration,
				PrimaryKey = configuration.Component.ToString()
			};

			Tenant.GetService<IStorageService>().Upload(blob, content, StoragePolicy.Singleton);
			InvalidateIndexState(configuration.Component);

			if (Tenant.GetService<IComponentService>() is IComponentNotification n)
				n.NotifyChanged(this, new ConfigurationEventArgs(c.MicroService, configuration.Component, c.Category));

			var u = Tenant.CreateUrl("NotificationDevelopment", "ConfigurationChanged");
			var args = new JObject
				{
					 { "configuration", c.Token }
				};

			Tenant.Post(u, args);
		}

		public void Update(IText text, string content)
		{
			Tenant.GetService<IDesignService>().VersionControl.Lock(text.Configuration().Component, Development.LockVerb.Edit);

			if (string.IsNullOrWhiteSpace(content))
				Delete(text, true);
			else
			{
				var s = Tenant.GetService<IMicroServiceService>().Select(text.Configuration().MicroService());
				var raw = Encoding.UTF8.GetBytes(content);

				var b = new Blob
				{
					ContentType = "application/json",
					FileName = text.FileName,
					PrimaryKey = text.Id.ToString(),
					Size = content.Length,
					MicroService = s.Token,
					ResourceGroup = s.ResourceGroup,
					Type = BlobTypes.Template
				};

				var blob = Tenant.GetService<IStorageService>().Upload(b, raw, StoragePolicy.Singleton);

				if (text.TextBlob != blob)
					text.TextBlob = blob;

				Update(text.Configuration());
				InvalidateIndexState(text);
			}
		}

		private void Delete(IText text, bool updateConfig)
		{
			if (text.TextBlob == Guid.Empty)
				return;

			try
			{
				Tenant.GetService<IStorageService>().Delete(text.TextBlob);
				Tenant.GetService<IDesignService>().Search.Delete(text.Configuration().Component, text.Id);
			}
			catch { }

			text.TextBlob = Guid.Empty;

			if (updateConfig)
				Update(text.Configuration());
		}

		private void RemoveDependencies(Guid component)
		{
			try
			{
				var config = Tenant.GetService<IComponentService>().SelectConfiguration(component);

				if (config == null)
					return;

				var txt = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

				foreach (var i in txt)
					Delete(i, false);

				var external = Tenant.GetService<IDiscoveryService>().Configuration.Query<IExternalResourceElement>(config);

				foreach (var i in external)
					i.Clean(i.Id);
			}
			catch (Exception ex)
			{
				Tenant.LogWarning(ex.Source, ex.Message, LogCategories.Development);
			}
		}

		public void DeleteFolder(Guid microService, Guid folder, bool deleteComponents)
		{
			var folders = Tenant.GetService<IComponentService>().QueryFolders(microService, folder);

			foreach (var i in folders)
				DeleteFolder(microService, i.Token, deleteComponents);

			var components = Tenant.GetService<IComponentService>().QueryComponents(microService, folder);

			if (deleteComponents)
			{
				foreach (var i in components)
					Delete(i.Token);
			}
			else
			{
				foreach (var component in components)
					Update(component.Token, component.Name, Guid.Empty);
			}

			var u = Tenant.CreateUrl("FolderDevelopment", "Delete");
			var args = new JObject
				{
					 {"microService", microService },
					 { "token", folder }
				};

			Tenant.Post(u, args);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyFolderRemoved(this, new FolderEventArgs(microService, folder));
		}

		public void RestoreFolder(Guid microService, Guid token, string name, Guid parent)
		{
			var u = Tenant.CreateUrl("FolderDevelopment", "Restore");
			var args = new JObject
				{
					 {"microService", microService },
					 { "name", name },
					 { "parent", parent },
					 { "token", token }
				};

			Tenant.Post(u, args);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyFolderChanged(this, new FolderEventArgs(microService, token));
		}

		public Guid InsertFolder(Guid microService, string name, Guid parent)
		{
			var u = Tenant.CreateUrl("FolderDevelopment", "Insert");
			var args = new JObject
				{
					 {"microService", microService },
					 { "name", name },
					 { "parent", parent }
				};

			var r = Tenant.Post<Guid>(u, args);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyFolderChanged(this, new FolderEventArgs(microService, r));

			return r;
		}

		public void UpdateFolder(Guid microService, Guid folder, string name, Guid parent)
		{
			var u = Tenant.CreateUrl("FolderDevelopment", "Update");
			var args = new JObject
				{
					 {"microService", microService },
					 { "token", folder },
					 { "name", name },
					 { "parent", parent }
				};

			Tenant.Post(u, args);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyFolderChanged(this, new FolderEventArgs(microService, folder));
		}

		public IComponentImage CreateComponentImage(Guid component)
		{
			var c = Tenant.GetService<IComponentService>().SelectComponent(component);
			var r = new ComponentImage
			{
				Category = c.Category,
				Folder = c.Folder,
				MicroService = c.MicroService,
				Name = c.Name,
				RuntimeConfiguration = c.RuntimeConfiguration,
				Token = c.Token,
				Type = c.Type,
				NameSpace = c.NameSpace,
				Configuration = CreateImageBlob(component)
			};

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(c.Token);

			if (config != null)
			{
				var deps = Tenant.GetService<IDiscoveryService>().Configuration.QueryDependencies(config);

				foreach (var j in deps)
					r.Dependencies.Add(CreateImageBlob(j));
			}

			return r;
		}

		private IComponentImageBlob CreateImageBlob(Guid blob)
		{
			var b = Tenant.GetService<IStorageService>().Select(blob);

			if (b == null)
				return null;

			return new ComponentImageBlob
			{
				Content = Tenant.GetService<IStorageService>().Download(b.Token)?.Content,
				Token = b.Token,
				ContentType = b.ContentType,
				FileName = b.FileName,
				PrimaryKey = b.PrimaryKey,
				Topic = b.Topic,
				Type = b.Type,
				Version = b.Version
			};
		}

		public IComponentImage SelectComponentImage(Guid blob)
		{
			var content = Tenant.GetService<IStorageService>().Download(blob);

			if (content == null)
				return null;

			return Tenant.GetService<ISerializationService>().Deserialize(content.Content, typeof(ComponentImage)) as ComponentImage;
		}

		public void RestoreComponent(IComponentImage image)
		{
			var component = Tenant.GetService<IComponentService>().SelectComponent(image.Token);
			var folder = image.Folder;
			/*
		 * if previous folder doesn't exist anymore we'll put component at root
		 */
			if (folder != Guid.Empty)
			{
				var f = Tenant.GetService<IComponentService>().SelectFolder(image.Folder);

				if (f == null)
					folder = Guid.Empty;
			}
			/*
		 * if component doesn't exist it has probably been deleted so we must create
		 * a new component with the same identifiers
		 */
			if (component == null)
			{
				/*
			* runtime configuration has been lost when deleting. it currently cannot be restored.
			*/
				var u = Tenant.CreateUrl("ComponentDevelopment", "Insert");

				var args = new JObject
					 {
						  {"microService", image.MicroService },
						  {"folder", folder },
						  {"name", image.Name },
						  {"type", image.Type },
						  {"category", image.Category },
						  {"component", image.Token },
						  {"nameSpace", ComponentCategories.ResolveNamespace( image.Category) }
					 };

				Tenant.Post(u, args);

				component = Tenant.GetService<IComponentService>().SelectComponent(image.Token);
			}
			else
				Update(image.Token, image.Name, folder);

			var ms = Tenant.GetService<IMicroServiceService>().Select(component.MicroService);
			/*
		 * we need to find out if and blobs has been created. if so, delete it because we
		 * are performing undo or rollback.
		 */
			var config = Tenant.GetService<IComponentService>().SelectConfiguration(image.Token);
			var deps = Tenant.GetService<IDiscoveryService>().Configuration.QueryDependencies(config);

			foreach (var i in deps)
			{
				if (image.Dependencies.FirstOrDefault(f => f.Token == i) == null)
					Tenant.GetService<IStorageService>().Delete(i);
			}

			/*
		 * now restore configuration and all blobs
		 */
			var imageConfig = Tenant.GetService<ISerializationService>().Deserialize(image.Configuration.Content, Type.GetType(image.Type)) as IConfiguration;

			Update(imageConfig, new ComponentUpdateArgs(false));

			foreach (var i in image.Dependencies)
			{
				Tenant.GetService<IStorageService>().Upload(new Blob
				{
					ContentType = i.ContentType,
					FileName = i.FileName,
					MicroService = component.MicroService,
					PrimaryKey = i.PrimaryKey,
					ResourceGroup = ms.ResourceGroup,
					Token = i.Token,
					Topic = i.Topic,
					Type = i.Type,
					Version = i.Version
				}, i.Content, StoragePolicy.Singleton);
			}

			var sources = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(imageConfig);

			foreach (var source in sources)
				InvalidateIndexState(source);

			if (Tenant.GetService<IComponentService>() is IComponentNotification notification)
			{
				notification.NotifyChanged(this, new ComponentEventArgs
				{
					Category = component.Category,
					Component = component.Token,
					Folder = component.Folder,
					MicroService = component.MicroService,
					Name = component.Name,
					NameSpace = ComponentCategories.ResolveNamespace(component.Category)
				});
			}
		}

		public void RestoreComponent(Guid blob)
		{
			RestoreComponent(SelectComponentImage(blob));
		}

		public List<IComponent> Query(Guid microService)
		{
			var u = Tenant.CreateUrl("Component", "QueryAll")
				 .AddParameter("microService", microService);

			return Tenant.Get<List<Component>>(u).ToList<IComponent>();
		}

		public List<IComponent> Query(Guid[] microServices)
		{
			var u = Tenant.CreateUrl("Component", "QueryForMicroServices");
			var e = new JObject();
			var a = new JArray();

			e.Add("microServices", a);

			foreach (var microService in microServices)
				a.Add(microService);

			return Tenant.Post<List<Component>>(u, e).ToList<IComponent>();
		}

		private void InvalidateIndexState(Guid component)
		{
			UpdateIndexStates(new List<IComponentIndexState>
			{
				new ComponentIndexState
				{
					Component = Tenant.GetService<IComponentService>().SelectComponent(component),
					State = IndexState.Invalidated,
					TimeStamp = DateTime.UtcNow
				}
			});
		}

		private void InvalidateIndexState(IText element)
		{
			UpdateIndexStates(new List<IComponentIndexState>
			{
				new ComponentIndexState
				{
					Component = Tenant.GetService<IComponentService>().SelectComponent(element.Configuration().Component),
					Element = element.Id,
					State = IndexState.Invalidated,
					TimeStamp = DateTime.UtcNow
				}
			});
		}

		public void UpdateIndexStates(List<IComponentIndexState> states)
		{
			var u = Tenant.CreateUrl("ComponentDevelopment", "UpdateIndexStates");
			var e = new JObject();
			var a = new JArray();

			e.Add("items", a);

			foreach (var state in states)
			{
				a.Add(new JObject
				{
					{"component", state.Component.Token },
					{"element", state.Element },
					{"state", state.State.ToString() },
					{"timestamp", state.TimeStamp }
				});
			}

			Tenant.Post(u, e);
		}

		public void UpdateAnalyzerStates(List<IComponentAnalyzerState> states)
		{
			var u = Tenant.CreateUrl("ComponentDevelopment", "UpdateAnalyzerStates");
			var e = new JObject();
			var a = new JArray();

			e.Add("items", a);

			foreach (var state in states)
			{
				e.Add(new JObject
				{
					{"component", state.Component.Token },
					{"element", state.Element },
					{"state", state.State.ToString() },
					{"timestamp", state.TimeStamp }
				});
			}

			Tenant.Post(u, e);
		}

		private ServerUrl CreateUrl(string action)
		{
			return Tenant.CreateUrl("ComponentDevelopment", action);
		}

		public void SaveRuntimeState(Guid microService)
		{
			var state = new JObject
				{
					 {"microService", microService }
				};

			var rt = new JArray();

			state.Add("runtimeConfigurations", rt);

			var components = Tenant.GetService<IComponentService>().QueryComponents(microService);

			foreach (var i in components)
			{
				if (i.RuntimeConfiguration != Guid.Empty)
				{
					rt.Add(new JObject
								{
									 {i.Token.ToString(), i.RuntimeConfiguration.ToString() }
								});
				}
			}

			if (rt.Count == 0)
				return;

			var u = Tenant.CreateUrl("ComponentDevelopment", "SaveRuntimeState");

			Tenant.Post(u, state);
		}

		public Dictionary<Guid, Guid> SelectRuntimeState(Guid microService)
		{
			var u = Tenant.CreateUrl("ComponentDevelopment", "SelectRuntimeState");
			var e = new JObject
				{
					 {"microService", microService }
				};

			var a = Tenant.Post<JArray>(u, e);

			if (a == null)
				return null;

			var r = new Dictionary<Guid, Guid>();

			foreach (JObject i in a)
			{
				var prop = i.First as JProperty;

				r.Add(new Guid(prop.Name), new Guid(prop.Value.ToString()));
			}

			return r;
		}

		public void DropRuntimeState(Guid microService)
		{
			var u = Tenant.CreateUrl("ComponentDevelopment", "DropRuntimeState");
			var e = new JObject
				{
					 {"microService", microService }
				};

			Tenant.Post(u, e);
		}
	}
}
