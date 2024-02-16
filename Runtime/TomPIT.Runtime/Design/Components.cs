using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Storage;
using TomPIT.Sys;

namespace TomPIT.Design
{
	internal class Components : TenantObject, IComponentModel
	{
		public event EventHandler<FileArgs> FileRestored;
		public event EventHandler<ComponentArgs> ComponentRestored;
		public event EventHandler<ComponentArgs> ConfigurationRestored;
		public event EventHandler<FileArgs> FileDeleted;
		public event EventHandler<ComponentArgs> MultiFilesSynchronized;

		public Components(ITenant tenant) : base(tenant)
		{
		}

		public string CreateName(Guid microService, string category, string prefix)
		{
			return Instance.SysProxy.Development.Components.CreateName(microService, ComponentCategories.ResolveNamespace(category), prefix);
		}

		public void Delete(Guid component)
		{
			var c = Tenant.GetService<IComponentService>().SelectComponent(component);

			if (c is null)
				return;

			var svc = Tenant.GetService<IComponentService>() as IComponentNotification;

			svc?.NotifyDeleting(this, new ComponentEventArgs(c.MicroService, c.Folder, component, c.NameSpace, c.Category, c.Name));

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(c.Token);

			if (config is not null)
			{
				var texts = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

				foreach (var text in texts)
					Delete(text, false);
			}

			RemoveDependencies(c.Token);

			if (config is IMultiFileElement multiFile)
			{
				AsyncUtils.RunSync(multiFile.ProcessDeleted);

				MultiFilesSynchronized?.Invoke(this, new ComponentArgs(c.MicroService, c.Token, c.Category));
			}

			Instance.SysProxy.Development.Components.Delete(component, MiddlewareDescriptor.Current.UserToken);

			svc?.NotifyRemoved(this, new ComponentEventArgs(c.MicroService, c.Folder, component, c.NameSpace, c.Category, c.Name));


			/*
			 * remove configuration file
			 */
			try
			{
				Instance.SysProxy.SourceFiles.Delete(c.MicroService, c.Token, Storage.BlobTypes.Configuration);
			}
			catch (SysException ex)
			when (ex.Message == SR.ErrBlobNotFound)
			{
				//Could not delete non existing blob. We want it gone anyway.
			}

			Instance.SysProxy.Development.Notifications.ConfigurationRemoved(c.MicroService, c.Token, c.Category);

			Tenant.GetService<IDebugService>().ConfigurationRemoved(c.Token);
		}

		public void Restore(Guid microService, IPullRequestComponent component)
		{
			if (component.Verb == ComponentVerb.Delete)
			{
				Delete(component.Token);
				NotifyRemoved(microService, component);

				return;
			}

			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			RestoreComponent(microService, component);
			RestoreConfiguration(ms, component);
			RestoreFiles(ms, component);

			ConfigurationRestored?.Invoke(this, new ComponentArgs(microService, component.Token, component.Category));
			ComponentRestored?.Invoke(this, new ComponentArgs(microService, component.Token, component.Category));

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);

			if (config is IMultiFileElement multiFile)
			{
				AsyncUtils.RunSync(multiFile.ProcessRestored);

				MultiFilesSynchronized?.Invoke(this, new ComponentArgs(ms.Token, component.Token, component.Category));
			}
		}

		private void RestoreConfiguration(IMicroService microService, IPullRequestComponent component)
		{
			var configuration = component.Files.FirstOrDefault(f => f.Type == Storage.BlobTypes.Configuration);

			if (configuration is null || configuration.Verb == ComponentVerb.Delete || configuration.Verb == ComponentVerb.NotModified)
				return;

			Instance.SysProxy.SourceFiles.Upload(microService.Token, component.Token, configuration.Type, component.Token.ToString(), configuration.FileName, configuration.ContentType, Unpack(configuration.Content), 1);
			Tenant.GetService<IDebugService>().ConfigurationChanged(configuration.Token);

			if (Tenant.GetService<IComponentService>() is not IComponentNotification notification)
				return;

			notification.NotifyChanged(this, new ConfigurationEventArgs
			{
				Category = component.Category,
				Component = component.Token,
				MicroService = microService.Token
			});
		}

		private void RestoreFiles(IMicroService microService, IPullRequestComponent component)
		{
			foreach (var file in component.Files)
			{
				if (file.Type == Storage.BlobTypes.Configuration || file.Verb == ComponentVerb.NotModified)
					continue;

				if (file.Verb == ComponentVerb.Delete)
				{
					if (IsSourceFile(file.Type))
						Instance.SysProxy.SourceFiles.Delete(microService.Token, file.Token, file.Type);
					else
						TomPIT.Tenant.GetService<IStorageService>().Delete(file.Token);

					if (file.Type == BlobTypes.SourceText && Tenant.GetService<ICompilerService>() is ICompilerNotification notification)
						notification.NotifyChanged(this, new SourceTextChangedEventArgs(microService.Token, component.Token, file.Token, file.Type));

					FileDeleted?.Invoke(this, new FileArgs(microService.Token, component.Token, file.Token));
				}
				else
				{
					var content = Unpack(file.Content);

					if (IsSourceFile(file.Type))
						Instance.SysProxy.SourceFiles.Upload(microService.Token, file.Token, file.Type, file.PrimaryKey, file.FileName, file.ContentType, content, 1);
					else
					{
						Tenant.GetService<IStorageService>().Restore(new Blob
						{
							ContentType = file.ContentType,
							FileName = file.FileName,
							MicroService = microService.Token,
							ResourceGroup = microService.ResourceGroup,
							Token = file.Token,
							PrimaryKey = file.PrimaryKey,
							Topic = file.Topic,
							Type = file.Type,
							Version = file.BlobVersion
						}, content);
					}

					if (file.Type == BlobTypes.SourceText && Tenant.GetService<ICompilerService>() is ICompilerNotification notification)
						notification.NotifyChanged(this, new SourceTextChangedEventArgs(microService.Token, component.Token, file.Token, file.Type));

					FileRestored?.Invoke(this, new FileArgs(microService.Token, component.Token, file.Token));
				}

				Tenant.GetService<IDebugService>().SourceTextChanged(microService.Token, component.Token, file.Token, file.Type);
			}
		}

		private void RestoreComponent(Guid microService, IPullRequestComponent component)
		{
			if (component.Verb == ComponentVerb.Add)
			{
				Instance.SysProxy.Development.Components.Insert(microService, component.Folder, component.Token, ComponentCategories.ResolveNamespace(component.Category), component.Category, component.Name, component.Type);

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
			}
			else
				Update(component.Token, component.Name, component.Folder);
		}
		private void NotifyRemoved(Guid microService, IPullRequestComponent component)
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
			if (string.IsNullOrEmpty(packed))
				return Array.Empty<byte>();

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

			if (Tenant.GetService<IComponentService>() is IComponentNotification n)
				n.NotifyChanged(this, new ConfigurationEventArgs(existing.MicroService, existing.Token, existing.Category));

			foreach (var element in elements)
				element.Reset();

			existingConfiguration.Component = newId;

			foreach (var blob in blobs)
			{
				if (blob.TextBlob == Guid.Empty)
					continue;

				var content = Instance.SysProxy.SourceFiles.Download(microService, blob.TextBlob, Storage.BlobTypes.Template);
				var text = content == null || content.Length == 0 ? string.Empty : Encoding.UTF8.GetString(content);

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
					var resourceBlob = Tenant.GetService<Storage.IStorageService>().Select(resource);

					if (resourceBlob == null)
						continue;

					var resourceBlobContent = Tenant.GetService<Storage.IStorageService>().Download(resourceBlob.Token);
					var newBlob = new Storage.Blob
					{
						ContentType = resourceBlob.ContentType,
						FileName = resourceBlob.FileName,
						MicroService = microService,
						PrimaryKey = external.Id.ToString(),
						ResourceGroup = ms.ResourceGroup,
						Type = resourceBlob.Type,
						Topic = resourceBlob.Topic
					};

					var token = Tenant.GetService<Storage.IStorageService>().Upload(newBlob, resourceBlobContent.Content, Storage.StoragePolicy.Singleton);

					external.Reset(resource, token);
				}
			}

			Update(existingConfiguration);

			return newId;
		}
		public Guid Insert(Guid microService, Guid folder, string category, string name, string type)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService) ?? throw new NotFoundException(SR.ErrMicroServiceNotFound);
			var t = TypeExtensions.GetType(type) ?? throw new TomPITException($"{SR.ErrCannotCreateComponentInstance} ({type})");
			var instance = t.CreateInstance<IConfiguration>() ?? throw new TomPITException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, type));

			instance.Component = Guid.NewGuid();
			instance.ComponentCreated();

			var content = Tenant.GetService<ISerializationService>().Serialize(instance);

			Instance.SysProxy.SourceFiles.Upload(microService, instance.Component, Storage.BlobTypes.Configuration, instance.Component.ToString(), $"{name}.json", "application/json", content, 1);
			Instance.SysProxy.Development.Components.Insert(microService, folder, instance.Component, ComponentCategories.ResolveNamespace(category), category, name, type);

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

			Instance.SysProxy.Development.Notifications.ConfigurationAdded(microService, instance.Component, category);
			Tenant.GetService<IDebugService>().ConfigurationAdded(instance.Component);

			if (instance is IMultiFileElement multiFile)
			{
				AsyncUtils.RunSync(multiFile.ProcessCreated);

				MultiFilesSynchronized?.Invoke(this, new ComponentArgs(microService, instance.Component, category));
			}

			return instance.Component;
		}
		public void Update(Guid component, string name, Guid folder)
		{
			var c = Tenant.GetService<IComponentService>().SelectComponent(component) ?? throw new TomPITException(SR.ErrComponentNotFound);

			Instance.SysProxy.Development.Components.Update(component, name, folder);

			if (Tenant.GetService<IComponentService>() is IComponentNotification n)
				n.NotifyChanged(this, new ConfigurationEventArgs(c.MicroService, component, c.Category));

			Tenant.GetService<IDebugService>().ConfigurationChanged(component);

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component);

			if (config is IMultiFileElement multiFile)
			{
				AsyncUtils.RunSync(multiFile.ProcessChanged);

				MultiFilesSynchronized?.Invoke(this, new ComponentArgs(c.MicroService, component, c.Category));
			}
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
			var c = Tenant.GetService<IComponentService>().SelectComponent(configuration.Component) ?? throw new TomPITException(SR.ErrComponentNotFound);
			var s = Tenant.GetService<IMicroServiceService>().Select(c.MicroService) ?? throw new TomPITException(SR.ErrMicroServiceNotFound);
			var content = Tenant.GetService<ISerializationService>().Serialize(configuration);

			Instance.SysProxy.SourceFiles.Upload(c.MicroService, configuration.Component, Storage.BlobTypes.Configuration, configuration.Component.ToString(), $"{c.Name}.json", "application/json", content, 1);

			if (Tenant.GetService<IComponentService>() is IComponentNotification n)
				n.NotifyChanged(this, new ConfigurationEventArgs(c.MicroService, configuration.Component, c.Category));

			Instance.SysProxy.Development.Notifications.ConfigurationChanged(c.MicroService, c.Token, c.Category);
			Tenant.GetService<IDebugService>().ConfigurationChanged(c.Token);

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(c.Token);

			if (config is IMultiFileElement multiFile)
			{
				AsyncUtils.RunSync(multiFile.ProcessChanged);

				MultiFilesSynchronized?.Invoke(this, new ComponentArgs(c.MicroService, c.Token, c.Category));
			}
		}

		public void Update(IText text, string content)
		{
			var s = Tenant.GetService<IMicroServiceService>().Select(text.Configuration().MicroService());
			var raw = Encoding.UTF8.GetBytes(content is null ? string.Empty : content);
			var token = text.TextBlob == Guid.Empty ? Guid.NewGuid() : text.TextBlob;

			Instance.SysProxy.SourceFiles.Upload(s.Token, token, Storage.BlobTypes.Template, text.Id.ToString(), text.FileName, "text/html", raw, 1);

			if (text.TextBlob != token)
				text.TextBlob = token;

			Update(text.Configuration());

			var component = Tenant.GetService<IComponentService>().SelectComponent(text.Configuration().Component);

			if (ComponentCategories.IsAssemblyCategory(component.Category))
				Tenant.GetService<IDesignService>().MicroServices.IncrementVersion(s.Token);

			Tenant.GetService<IDebugService>().SourceTextChanged(text.Configuration().MicroService(), text.Configuration().Component, text.TextBlob, BlobTypes.SourceText);

			if (Tenant.GetService<IComponentService>() is IComponentNotification cn)
				cn.NotifySourceTextChanged(Tenant, new SourceTextChangedEventArgs(text.Configuration().MicroService(), text.Configuration().Component, text.TextBlob, BlobTypes.SourceText));
		}

		private void Delete(IText text, bool updateConfig)
		{
			if (text.TextBlob == Guid.Empty)
				return;

			var blob = text.TextBlob;

			try
			{
				Instance.SysProxy.SourceFiles.Delete(text.Configuration().MicroService(), text.TextBlob, Storage.BlobTypes.Template);
				FileDeleted?.Invoke(this, new FileArgs(text.Configuration().MicroService(), text.Configuration().Component, text.Id));
			}
			catch { }

			text.TextBlob = Guid.Empty;

			if (updateConfig)
				Update(text.Configuration());

			Tenant.GetService<IDebugService>().SourceTextChanged(text.Configuration().MicroService(), text.Configuration().Component, text.TextBlob, BlobTypes.SourceText);
		}

		private void RemoveDependencies(Guid component)
		{
			try
			{
				var config = Tenant.GetService<IComponentService>().SelectConfiguration(component);

				if (config is null)
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

			Instance.SysProxy.Development.Folders.Delete(microService, folder);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyFolderRemoved(this, new FolderEventArgs(microService, folder));
		}

		public void RestoreFolder(Guid microService, Guid token, string name, Guid parent)
		{
			Instance.SysProxy.Development.Folders.Restore(microService, token, name, parent);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyFolderChanged(this, new FolderEventArgs(microService, token));
		}

		public Guid InsertFolder(Guid microService, string name, Guid parent)
		{
			var r = Instance.SysProxy.Development.Folders.Insert(microService, name, parent);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyFolderChanged(this, new FolderEventArgs(microService, r));

			return r;
		}

		public void UpdateFolder(Guid microService, Guid folder, string name, Guid parent)
		{
			Instance.SysProxy.Development.Folders.Update(microService, folder, name, parent);

			if (Tenant.GetService<IComponentService>() is IComponentNotification svc)
				svc.NotifyFolderChanged(this, new FolderEventArgs(microService, folder));
		}

		public IComponentImage CreateComponentImage(Guid component)
		{
			var c = Tenant.GetService<IComponentService>().SelectComponent(component);

			if (c is null)
				return null;

			var r = new ComponentImage
			{
				Category = c.Category,
				Folder = c.Folder,
				MicroService = c.MicroService,
				Name = c.Name,
				Token = c.Token,
				Type = c.Type,
				NameSpace = c.NameSpace,
				Configuration = CreateConfigurationBlob(component)
			};

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(c.Token);

			if (config != null)
			{
				var textFiles = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);
				var externalDeps = Tenant.GetService<IDiscoveryService>().Configuration.Query<IExternalResourceElement>(config);

				foreach (var j in textFiles)
				{
					if (j.TextBlob == Guid.Empty)
						continue;

					var blob = CreateSourceFileBlob(c.MicroService, j.TextBlob);

					if (blob is not null)
						r.Dependencies.Add(blob);
				}

				foreach (var external in externalDeps)
				{
					var resources = external.QueryResources();

					if (resources is null)
						continue;

					foreach (var resource in resources)
					{
						if (resource == Guid.Empty)
							continue;

						var blob = CreateExternalResourceBlob(resource);

						if (blob is null)
							continue;

						r.Dependencies.Add(blob);
					}
				}
			}

			return r;
		}

		private static IComponentImageBlob CreateSourceFileBlob(Guid microService, Guid token)
		{
			var b = Instance.SysProxy.SourceFiles.Select(token, Storage.BlobTypes.Template);

			if (b is null)
				return null;

			var content = Instance.SysProxy.SourceFiles.Download(microService, token, Storage.BlobTypes.Template);

			return new ComponentImageBlob
			{
				Content = content,
				Token = b.Token,
				ContentType = b.ContentType,
				FileName = b.FileName,
				PrimaryKey = b.PrimaryKey,
				Type = b.Type,
				Version = b.Version
			};
		}

		private static IComponentImageBlob CreateConfigurationBlob(Guid component)
		{
			var b = Instance.SysProxy.SourceFiles.Select(component, Storage.BlobTypes.Configuration);

			if (b is null)
				return null;

			var content = Instance.SysProxy.SourceFiles.Download(b.MicroService, component, Storage.BlobTypes.Configuration);

			return new ComponentImageBlob
			{
				Content = content,
				Token = b.Token,
				ContentType = b.ContentType,
				FileName = b.FileName,
				PrimaryKey = b.PrimaryKey,
				Type = b.Type,
				Version = b.Version
			};
		}

		private static IComponentImageBlob CreateExternalResourceBlob(Guid token)
		{
			var b = TomPIT.Tenant.GetService<Storage.IStorageService>().Select(token);

			if (b == null)
				return null;

			return new ComponentImageBlob
			{
				Content = TomPIT.Tenant.GetService<Storage.IStorageService>().Download(b.Token)?.Content,
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
			return CreateComponentImage(blob);
		}

		public List<IComponent> Query(Guid microService)
		{
			return Instance.SysProxy.Components.QueryAll(microService).ToList();
		}

		public List<IComponent> Query(Guid[] microServices)
		{
			return Instance.SysProxy.Components.QueryForMicroServices(microServices.ToList()).ToList();
		}

		public void Update(Guid microService, Guid token, int blobType, string contentType, string fileName, string primaryKey, byte[] content)
		{
			Instance.SysProxy.SourceFiles.Upload(microService, token, blobType, primaryKey, fileName, contentType, content, 1);
		}

		private bool IsSourceFile(int type)
		{
			return type == Storage.BlobTypes.Configuration || type == Storage.BlobTypes.Template || type == Storage.BlobTypes.AutoGenerated;
		}

		public void Delete(Guid microService, Guid token, int blobType)
		{
			Instance.SysProxy.SourceFiles.Delete(microService, token, blobType);
		}
	}
}
