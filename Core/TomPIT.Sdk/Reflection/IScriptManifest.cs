using System;
using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	public interface IScriptManifest
	{
		HashSet<IScriptManifestPointer> Pointers { get; }

		short Address { get; set; }

		List<short> ScriptReferences { get; }
		List<short> ResourceReferences { get; }
		Dictionary<IScriptManifestSymbolReference, HashSet<IScriptManifestSymbolLocation>> SymbolReferences { get; }

		List<IScriptManifestType> DeclaredTypes { get; }

		IScriptManifestPointer GetPointer(short id);
		short GetId(Guid microService, Guid component, Guid element);

		void LoadMetaData(ITenant tenant);
	}
}
