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
		private List<IScriptManifestType> _declaredTypes;
		private HashSet<IScriptManifestPointer> _pointers;
		private Dictionary<IScriptManifestSymbolReference, HashSet<IScriptManifestSymbolLocation>> _sourceReferences;
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
		[JsonConverter(typeof(ScriptManifestSymbolReferenceConverter))]
		public Dictionary<IScriptManifestSymbolReference, HashSet<IScriptManifestSymbolLocation>> SymbolReferences => _sourceReferences ??= new Dictionary<IScriptManifestSymbolReference, HashSet<IScriptManifestSymbolLocation>>(new ScriptManifestSymbolReferenceComparer());

		public List<IScriptManifestType> DeclaredTypes => _declaredTypes ??= new List<IScriptManifestType>();

		public HashSet<IScriptManifestPointer> Pointers => _pointers ??= new HashSet<IScriptManifestPointer>(new ScriptManifestPointerComparer());

		public short Address { get; set; }

		public short GetId(Guid microService, Guid component, Guid element)
		{
			var pointer = new ScriptManifestPointer
			{
				MicroService = microService,
				Component = component,
				Element = element
			};

			if (Pointers.TryGetValue(pointer, out IScriptManifestPointer existing))
				return existing.Id;

			pointer.Id = _identity++;

			Pointers.Add(pointer);

			return pointer.Id;
		}

		public IScriptManifestPointer GetPointer(short id)
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
