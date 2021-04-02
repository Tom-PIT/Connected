using System;
using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	public interface IScriptManifest
	{
		HashSet<IManifestPointer> Pointers { get; }

		short Address { get; set; }

		List<short> ScriptReferences { get; }
		List<short> ResourceReferences { get; }
		Dictionary<IManifestSymbolReference, HashSet<IManifestSymbolLocation>> SymbolReferences { get; }

		List<IManifestType> DeclaredTypes { get; }

		IManifestPointer GetPointer(short id);
		short GetId(Guid microService, Guid component, Guid element);

		void LoadMetaData(ITenant tenant);
	}
}
