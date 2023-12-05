using Lucene.Net.Messages;

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

         Instance.SysProxy.Development.Components.Delete(component, MiddlewareDescriptor.Current.UserToken);

         svc?.NotifyRemoved(this, new ComponentEventArgs(c.MicroService, c.Folder, component, c.NameSpace, c.Category, c.Name));

         /*
			 * remove configuration file
			 */
         try
         {
            Tenant.GetService<IStorageService>().Delete(c.Token);
         }
         catch (SysException ex)
         when (ex.Message == SR.ErrBlobNotFound)
         {
            //Could not delete non existing blob. We want it gone anyway.
         }

         Instance.SysProxy.Development.Notifications.ConfigurationRemoved(c.MicroService, c.Token, c.Category);

         Tenant.GetService<IDebugService>().ConfigurationRemoved(c.Token);

         if (config is IMultiFileElement multiFile)
         {
            AsyncUtils.RunSync(multiFile.ProcessDeleted);

            MultiFilesSynchronized?.Invoke(this, new ComponentArgs(c.MicroService, c.Token, c.Category));
         }
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
         var configuration = component.Files.FirstOrDefault(f => f.Type == BlobTypes.Configuration);

         if (configuration is null || configuration.Verb == ComponentVerb.Delete || configuration.Verb == ComponentVerb.NotModified)
            return;

         var blob = new Blob
         {
            ContentType = configuration.ContentType,
            FileName = configuration.FileName,
            ResourceGroup = microService.ResourceGroup,
            MicroService = microService.Token,
            Type = configuration.Type,
            Token = component.Token,
            PrimaryKey = component.Token.ToString()
         };

         Tenant.GetService<IStorageService>().Upload(blob, Unpack(configuration.Content), StoragePolicy.Singleton, component.Token);
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
            if (file.Type == BlobTypes.Configuration || file.Verb == ComponentVerb.NotModified)
               continue;

            var elementId = ResolveElementId(component.Token, file.Token);

            if (file.Verb == ComponentVerb.Delete)
            {
               Tenant.GetService<IStorageService>().Delete(file.Token);

               if (file.Type == BlobTypes.Template && Tenant.GetService<ICompilerService>() is ICompilerNotification notification)
                  notification.NotifyChanged(this, new ScriptChangedEventArgs(microService.Token, component.Token, elementId));

               FileDeleted?.Invoke(this, new FileArgs(microService.Token, component.Token, file.Token));
            }
            else
            {
               var content = Unpack(file.Content);

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

               if (file.Type == BlobTypes.Template && Tenant.GetService<ICompilerService>() is ICompilerNotification notification)
                  notification.NotifyChanged(this, new ScriptChangedEventArgs(microService.Token, component.Token, ResolveElementId(component.Token, file.Token)));

               FileRestored?.Invoke(this, new FileArgs(microService.Token, component.Token, file.Token));
            }

            Tenant.GetService<IDebugService>().ScriptChanged(microService.Token, component.Token, elementId, file.Token);
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
      private Guid ResolveElementId(Guid component, Guid blob)
      {
         return Tenant.GetService<IDiscoveryService>().Configuration.Find(component, blob, SearchMode.Blob)?.Id ?? default;
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
         var ms = Tenant.GetService<IMicroServiceService>().Select(microService) ?? throw new NotFoundException(SR.ErrMicroServiceNotFound);
         var t = TypeExtensions.GetType(type) ?? throw new TomPITException($"{SR.ErrCannotCreateComponentInstance} ({type})");
         var instance = t.CreateInstance<IConfiguration>() ?? throw new TomPITException(string.Format("{0} ({1})", SR.ErrCannotCreateComponentInstance, type));

         instance.Component = Guid.NewGuid();
         instance.ComponentCreated();

         var content = Tenant.GetService<ISerializationService>().Serialize(instance);

         var blob = new Blob
         {
            ContentType = "application/json",
            Draft = Guid.NewGuid().ToString(),
            FileName = string.Format("{0}.json", name),
            ResourceGroup = ms.ResourceGroup,
            Size = content.Length,
            MicroService = microService,
            Type = BlobTypes.Configuration,
            Token = instance.Component
         };

         Tenant.GetService<IStorageService>().Upload(blob, content, StoragePolicy.Singleton, instance.Component);
         Instance.SysProxy.Development.Components.Insert(microService, folder, instance.Component, ComponentCategories.ResolveNamespace(category), category, name, type);
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

         var component = Tenant.GetService<IComponentService>().SelectComponent(text.Configuration().Component);

         if (ComponentCategories.IsAssemblyCategory(component.Category))
            Tenant.GetService<IDesignService>().MicroServices.IncrementVersion(s.Token);

         Tenant.GetService<IDebugService>().ScriptChanged(text.Configuration().MicroService(), text.Configuration().Component, text.Id, blob);
      }

      private void Delete(IText text, bool updateConfig)
      {
         if (text.TextBlob == Guid.Empty)
            return;

         var blob = text.TextBlob;

         try
         {
            Tenant.GetService<IStorageService>().Delete(text.TextBlob);

            FileDeleted?.Invoke(this, new FileArgs(text.Configuration().MicroService(), text.Configuration().Component, text.Id));
         }
         catch { }

         text.TextBlob = Guid.Empty;

         if (updateConfig)
            Update(text.Configuration());

         Tenant.GetService<IDebugService>().ScriptChanged(text.Configuration().MicroService(), text.Configuration().Component, text.Id, blob);
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

      public List<IComponent> Query(Guid microService)
      {
         return Instance.SysProxy.Components.QueryAll(microService).ToList();
      }

      public List<IComponent> Query(Guid[] microServices)
      {
         return Instance.SysProxy.Components.QueryForMicroServices(microServices.ToList()).ToList();
      }
   }
}
