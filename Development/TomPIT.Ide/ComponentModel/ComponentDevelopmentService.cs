﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design.Serialization;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Ide.Search;
using TomPIT.Ide.VersionControl;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.Ide.ComponentModel
{
	internal class ComponentDevelopmentService : TenantObject, IComponentDevelopmentService
	{
		public ComponentDevelopmentService(ITenant tenant) : base(tenant)
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
				throw new IdeException(SR.ErrComponentNotFound);

			if (!permanent)
				Tenant.GetService<IVersionControlService>().Lock(component, Development.LockVerb.Delete);

			if (permanent)
				RemoveDependencies(c.Token);

			var u = Tenant.CreateUrl("ComponentDevelopment", "Delete");
			var args = new JObject
				{
					 {"component", component },
					 {"permanent", permanent },
					 {"user", MiddlewareDescriptor.Current.UserToken }
				};

			Tenant.Post(u, args);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyRemoved(this, new ComponentEventArgs(c.MicroService, c.Folder, component, c.Category));

			/*
		 * remove configuration file
		 */
			if (permanent)
			{
				Tenant.GetService<IStorageService>().Delete(c.Token);

				if (c.RuntimeConfiguration != Guid.Empty)
					Tenant.GetService<IStorageService>().Delete(c.RuntimeConfiguration);

				Tenant.GetService<IIdeSearchService>().Delete(c.Token);
			}

			u = Tenant.CreateUrl("NotificationDevelopment", "ConfigurationRemoved");
			args = new JObject
				{
					 { "configuration", c.Token},
					 { "microService", c.MicroService},
					 { "category", c.Category}
				};

			Tenant.Post(u, args);
		}

		public void Restore(Guid microService, IPackageComponent component, IPackageBlob configuration, IPackageBlob runtimeConfiguration)
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

			if (runtimeConfigurationId != Guid.Empty)
			{
				if (runtimeConfiguration != null)
				{
					blob = new Blob
					{
						ContentType = runtimeConfiguration.ContentType,
						FileName = runtimeConfiguration.FileName,
						ResourceGroup = ms.ResourceGroup,
						MicroService = microService,
						Type = runtimeConfiguration.Type,
						Token = runtimeConfiguration.Token,
						PrimaryKey = runtimeConfiguration.PrimaryKey
					};

					Tenant.GetService<IStorageService>().Upload(blob, Convert.FromBase64String(runtimeConfiguration.Content), StoragePolicy.Singleton, runtimeConfigurationId);
				}
				else
				{
					var stateBlob = Tenant.GetService<IStorageService>().Select(runtimeConfigurationId);

					if (stateBlob != null)
					{
						var state = Tenant.GetService<IStorageService>().Download(runtimeConfigurationId);
						runtimeConfigurationId = Guid.NewGuid();

						if (state != null)
						{
							blob = new Blob
							{
								ContentType = stateBlob.ContentType,
								FileName = stateBlob.FileName,
								ResourceGroup = ms.ResourceGroup,
								MicroService = microService,
								Type = BlobTypes.RuntimeConfiguration,
								PrimaryKey = runtimeConfigurationId.ToString(),
								Token = runtimeConfigurationId
							};

							Tenant.GetService<IStorageService>().Restore(blob, state.Content);
						}
					}
				}
			}

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
			InvalidateIndexState(component.Token);
		}

		public Guid Insert(Guid microService, Guid folder, string category, string name, string type)
		{
			var s = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (s == null)
				throw new IdeException(SR.ErrMicroServiceNotFound);

			var t = Reflection.TypeExtensions.GetType(type);

			if (t == null)
				throw new IdeException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, type));

			var instance = t.CreateInstance<IConfiguration>();

			if (instance == null)
				throw new IdeException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, type));

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
			InvalidateIndexState(instance.Component);

			u = Tenant.CreateUrl("NotificationDevelopment", "ConfigurationAdded");
			args = new JObject
				{
					 { "configuration", instance.Component }
				};

			Tenant.Post(u, args);
			Tenant.GetService<IVersionControlService>().Lock(instance.Component, Development.LockVerb.Add);

			return instance.Component;
		}

		public void Update(Guid component, string name, Guid folder, bool performLock)
		{
			if (performLock)
				Tenant.GetService<IVersionControlService>().Lock(component, Development.LockVerb.Edit);

			var u = Tenant.CreateUrl("ComponentDevelopment", "Update");
			var args = new JObject
				{
					 {"name", name },
					 {"component", component },
					 {"folder", folder }
				};

			Tenant.Post(u, args);
			InvalidateIndexState(component);
		}

		public void Update(Guid component, string name, Guid folder)
		{
			Update(component, name, folder, true);
		}

		public void Update(IConfiguration configuration)
		{
			UpdateConfiguration(configuration, new ComponentUpdateArgs(Shell.GetService<IRuntimeService>().Mode, true));
		}

		public void Update(IConfiguration configuration, ComponentUpdateArgs e)
		{
			UpdateConfiguration(configuration, e);
		}

		private void UpdateConfiguration(IConfiguration configuration, ComponentUpdateArgs e)
		{
			var c = Tenant.GetService<IComponentService>().SelectComponent(configuration.Component);

			if (c == null)
				throw new IdeException(SR.ErrComponentNotFound);

			var s = Tenant.GetService<IMicroServiceService>().Select(c.MicroService);

			if (s == null)
				throw new IdeException(SR.ErrMicroServiceNotFound);
			/*
		 * version control lock needs to be obtained only for design time
		 */
			if (e.Mode == EnvironmentMode.Design && e.PerformLock)
				Tenant.GetService<IVersionControlService>().Lock(c.Token, Development.LockVerb.Edit);

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

			if (e.Mode == EnvironmentMode.Runtime)
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

				Tenant.GetService<IStorageService>().Upload(blob, content, StoragePolicy.Singleton, rt);
			}
			else
			{
				Tenant.GetService<IStorageService>().Upload(blob, content, StoragePolicy.Singleton);
				InvalidateIndexState(configuration.Component);
			}

			if (Tenant.GetService<IComponentService>() is IComponentNotification n)
				n.NotifyChanged(this, new ConfigurationEventArgs(c.MicroService, configuration.Component, c.Category));

			var u = Tenant.CreateUrl("NotificationDevelopment", "ConfigurationChanged");
			var args = new JObject
				{
					 { "configuration", c.Token }
				};

			Tenant.Post(u, args);
		}
		private void UpdateRuntimeConfiguration(Guid component, Guid runtimeConfiguration)
		{
			var u = Tenant.CreateUrl("ComponentDevelopment", "UpdateRuntimeConfiguration");
			var args = new JObject
				{
					 {"runtimeConfiguration", runtimeConfiguration },
					 {"component", component }
				};

			Tenant.Post(u, args);
		}

		public void Update(IText text, string content)
		{
			Tenant.GetService<IVersionControlService>().Lock(text.Configuration().Component, Development.LockVerb.Edit);

			if (string.IsNullOrWhiteSpace(content))
				Delete(text);
			else
			{
				var s = Tenant.GetService<IMicroServiceService>().Select(text.Configuration().MicroService());
				var raw = Encoding.UTF8.GetBytes(content);

				var b = new Blob
				{
					ContentType = "application/json",
					FileName = string.Format("{0}.json", text.Id.ToString()),
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

		private void Delete(IText text)
		{
			if (text.TextBlob == Guid.Empty)
				return;

			try
			{
				Tenant.GetService<IStorageService>().Delete(text.TextBlob);
				Tenant.GetService<IIdeSearchService>().Delete(text.Configuration().Component, text.Id);
			}
			catch { }

			text.TextBlob = Guid.Empty;

			Update(text.Configuration());
		}

		private void RemoveDependencies(Guid component)
		{
			try
			{
				var config = Tenant.GetService<IComponentService>().SelectConfiguration(component);

				if (config == null)
					return;

				var txt = config.Children<IText>();

				foreach (var i in txt)
					Delete(i);

				var external = config.Children<IExternalResourceElement>();

				foreach (var i in external)
					i.Clean(i.Id);
			}
			catch (Exception ex)
			{
				Tenant.LogWarning(ex.Source, ex.Message, LogCategories.Development);
			}
		}

		public void DeleteFolder(Guid microService, Guid folder)
		{
			var folders = Tenant.GetService<IComponentService>().QueryFolders(microService, folder);

			foreach (var i in folders)
				DeleteFolder(microService, i.Token);

			var components = Tenant.GetService<IComponentService>().QueryComponents(microService, folder);

			foreach (var i in components)
				Delete(i.Token);

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
				Configuration = CreateImageBlob(component)
			};

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(c.Token);

			if (config != null)
			{
				var deps = config.Dependencies();

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

		private ComponentImage DownloadImage(Guid blob)
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
			var deps = config.Dependencies();

			foreach (var i in deps)
			{
				if (image.Dependencies.FirstOrDefault(f => f.Token == i) == null)
					Tenant.GetService<IStorageService>().Delete(i);
			}

			/*
		 * now restore configuration and all blobs
		 */
			var imageConfig = Tenant.GetService<ISerializationService>().Deserialize(image.Configuration.Content, Type.GetType(image.Type)) as IConfiguration;

			Update(imageConfig, new ComponentUpdateArgs(Shell.GetService<IRuntimeService>().Mode, false));

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

			var sources = imageConfig.Children<IText>();

			foreach (var source in sources)
				InvalidateIndexState(source);
		}

		public void RestoreComponent(Guid blob)
		{
			RestoreComponent(DownloadImage(blob));
		}

		public void Import(Guid microService, Guid blob)
		{
			throw new NotImplementedException();

			//var image = await DownloadImageAsync(blob);
			//var imageConfig = Tenant.GetService<ISerializationService>().Deserialize(image.Configuration.Content, Type.GetType(image.Type)) as IConfiguration;
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
	}
}