using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
		private SingletonProcessor<Guid> _preloadProcessor = new SingletonProcessor<Guid>();
		private bool _initialized = false;
		public ScriptManifestCache(ITenant tenant) : base(tenant, nameof(ScriptManifestCache))
		{
		}

		public IScriptManifest Rebuild(Guid microService, Guid component, Guid id)
		{
			RemoveManifest(id);

			var compiler = new ScriptManifestCompiler(Tenant, microService, component, id);

			compiler.Compile();
			Save(microService, compiler.Script, compiler.Manifest);

			return compiler.Manifest;
		}

		public void RemoveManifest(Guid id)
		{
			var existing = Get(id);

			Remove(id);

			if (existing is not null)
			{
				var address = existing.GetPointer(existing.Address);

				Load(address.MicroService, address.Component, address.Element);
			}
		}

		public IScriptManifest Select(Guid microService, Guid component, Guid id)
		{
			return Get(id, (f) =>
			{
				f.Duration = TimeSpan.Zero;

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
			IScriptManifest manifest = null;
			
			PreloadProcessor.Start(id,
				() =>
				{
					var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

					if (Load(Tenant.GetService<IStorageService>().Download(microService, BlobTypes.ScriptManifest, ms.ResourceGroup, id.ToString())) is IScriptManifest result)
						manifest = result;
					else
					{
						var element = Tenant.GetService<IDiscoveryService>().Configuration.Find(component, id);

						manifest = Rebuild(element.Configuration().MicroService(), component, id);
					}
				},
				() =>
				{
					manifest = Get(id);
				});

			return manifest;
		}

		private IScriptManifest Load(IBlobContent content)
		{
			if (content?.Content is null || content.Content.Length == 0)
				return null;

			try
			{
				var serializer = new ScriptManifestDeserializer();

				serializer.Deserialize(content.Content);

				/*
				 * couldn't deserialize. possibly breaking change.
				 */
				if (serializer.IsEmpty)
					return null;

				return serializer.Manifest;
			}
			catch
			{

			}
			finally
			{
				Tenant.GetService<IStorageService>().Release(content.Blob);
			}

			return null;
		}

		private void Preload(Guid component)
		{
			if (_initialized)
				return;

			PreloadProcessor.Start(Guid.Empty,
				() =>
				{
					if (_initialized)
						return;

					_initialized = true;

					if (Tenant.GetService<IRuntimeService>().Stage == EnvironmentStage.Development)
						Tenant.GetService<IStorageService>().Preload(BlobTypes.ScriptManifest);
					else
					{
						if (Tenant.GetService<IComponentService>().SelectComponent(component) is IComponent c)
							Tenant.GetService<IStorageService>().Preload(BlobTypes.ScriptManifest, c.MicroService);
					}
				});
		}

		public IImmutableList<IScriptManifest> QueryReferences(IScriptManifest e)
		{
			var result = new List<IScriptManifest>();
			var address = e.GetPointer(e.Address);

			foreach(var manifest in All())
			{
				if (manifest == e)
					continue;

				if (manifest.Pointers.Contains(address))
					result.Add(manifest);
			}

			return result.ToImmutableList();
		}

		private SingletonProcessor<Guid> PreloadProcessor => _preloadProcessor;

		//private void CreateImages(ImmutableList<Guid> blobs)
		//{
		//	foreach (var blob in blobs)
		//	{
		//		if (Load(Tenant.GetService<IStorageService>().Download(blob)) is IScriptManifest manifest)
		//			Set(manifest.GetPointer(manifest.Address).Element, manifest, TimeSpan.Zero);
		//	}
		//}
	}
}