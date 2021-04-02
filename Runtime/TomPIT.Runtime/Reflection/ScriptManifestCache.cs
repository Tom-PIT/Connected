using System;
using System.Text;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Runtime;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.Reflection
{
	internal class ScriptManifestCache : ClientRepository<IScriptManifest, Guid>
	{
		public ScriptManifestCache(ITenant tenant) : base(tenant, nameof(ScriptManifestCache))
		{
		}

		public void Rebuild(Guid microService, Guid component, Guid id)
		{
			RemoveManifest(id);

			var compiler = new ScriptManifestCompiler(Tenant, microService, component, id);

			compiler.Compile();
			Save(microService, compiler.Script, compiler.Manifest);
		}

		public void RemoveManifest(Guid id)
		{
			Remove(id);
		}

		public IScriptManifest Select(Guid microService, Guid component, Guid id)
		{
			return Get(id, (f) =>
			{
				return Load(microService, component, id);
			});
		}

		private void Save(Guid microService, IText script, IScriptManifest manifest)
		{
			if (manifest == null)
				return;

			var pointer = manifest.GetPointer(manifest.Address);
			var serializer = new ScriptManifestSerializer();
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			Set(pointer.Element, manifest, TimeSpan.Zero);

			var blob = new Blob
			{
				ContentType = "application/json",
				FileName = script.FileName,
				MicroService = microService,
				PrimaryKey = pointer.Element.ToString(),
				ResourceGroup = ms.ResourceGroup,
				Type = BlobTypes.ScriptManifest
			};

			var dblob = new Blob
			{
				ContentType = "application/json",
				FileName = script.FileName,
				MicroService = microService,
				PrimaryKey = pointer.Element.ToString(),
				ResourceGroup = ms.ResourceGroup,
				Type = BlobTypes.ScriptManifestMetaData
			};

			serializer.Serialize(manifest);

			Tenant.GetService<IStorageService>().Upload(blob, Encoding.UTF8.GetBytes(Serializer.Serialize(serializer.Container)), StoragePolicy.Singleton);
			Tenant.GetService<IStorageService>().Upload(dblob, Encoding.UTF8.GetBytes(Serializer.Serialize(serializer.MetaData)), StoragePolicy.Singleton);
		}

		private IScriptManifest Load(Guid microService, Guid component, Guid id)
		{
			Preload(component);

			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);
			var content = Tenant.GetService<IStorageService>().Download(microService, BlobTypes.ScriptManifest, ms.ResourceGroup, id.ToString());

			if (content?.Content != null && content.Content.Length > 0)
			{
				try
				{
					var serializer = new ScriptManifestDeserializer();

					serializer.Deserialize(content.Content);
					/*
					 * couldn't deserialize. possibly breaking change.
					 */
					if (serializer.IsEmpty)
						Tenant.GetService<IStorageService>().Delete(content.Blob);
					else
						return serializer.Manifest;
				}
				catch
				{

				}
			}

			var element = Tenant.GetService<IDiscoveryService>().Configuration.Find(component, id);

			Rebuild(element.Configuration().MicroService(), component, id);

			return Get(id);
		}

		private void Preload(Guid component)
		{
			if (Tenant.GetService<IRuntimeService>().Stage == EnvironmentStage.Development)
				Tenant.GetService<IStorageService>().Preload(BlobTypes.ScriptManifest);
			else
			{
				if (Tenant.GetService<IComponentService>().SelectComponent(component) is IComponent c)
					Tenant.GetService<IStorageService>().Preload(BlobTypes.ScriptManifest, c.MicroService);
			}
		}
	}
}