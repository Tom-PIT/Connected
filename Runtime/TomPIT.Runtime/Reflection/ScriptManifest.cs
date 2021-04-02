using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Storage;

namespace TomPIT.Reflection
{
	internal class ScriptManifest : IScriptManifest
	{
		private List<short> _scriptReferences;
		private List<short> _resourceReferences;
		private List<IManifestType> _declaredTypes;
		private HashSet<IManifestPointer> _pointers;
		private Dictionary<IManifestSymbolReference, HashSet<IManifestSymbolLocation>> _sourceReferences;
		private short _identity = 0;
		private bool _metaDataLoaded = false;

		public ScriptManifest()
		{

		}

		public ScriptManifest(bool metaDataLoaded)
		{
			_metaDataLoaded = metaDataLoaded;
		}
		public List<short> ScriptReferences => _scriptReferences ??= new List<short>();
		public List<short> ResourceReferences => _resourceReferences ??= new List<short>();
		[JsonConverter(typeof(ManifestSymbolReferenceConverter))]
		public Dictionary<IManifestSymbolReference, HashSet<IManifestSymbolLocation>> SymbolReferences => _sourceReferences ??= new Dictionary<IManifestSymbolReference, HashSet<IManifestSymbolLocation>>(new ManifestSymbolReferenceComparer());

		public List<IManifestType> DeclaredTypes => _declaredTypes ??= new List<IManifestType>();

		public HashSet<IManifestPointer> Pointers => _pointers ??= new HashSet<IManifestPointer>(new ManifestPointerComparer());

		public short Address { get; set; }

		public short GetId(Guid microService, Guid component, Guid element)
		{
			var pointer = new ManifestPointer
			{
				MicroService = microService,
				Component = component,
				Element = element
			};

			if (Pointers.TryGetValue(pointer, out IManifestPointer existing))
				return existing.Id;

			pointer.Id = _identity++;

			Pointers.Add(pointer);

			return pointer.Id;
		}

		public IManifestPointer GetPointer(short id)
		{
			return Pointers.FirstOrDefault(f => f.Id == id);
		}

		public void LoadMetaData(ITenant tenant)
		{
			if (_metaDataLoaded)
				return;

			_metaDataLoaded = true;

			var address = GetPointer(Address);

			if (address == null)
				return;

			var ms = tenant.GetService<IMicroServiceService>().Select(address.MicroService);
			var content = tenant.GetService<IStorageService>().Download(address.MicroService, BlobTypes.ScriptManifestMetaData, ms.ResourceGroup, address.Element.ToString());

			if (content?.Content != null)
				new ScriptMetaDataDeserializer().Deserialize(this, content.Content);
		}
	}
}
