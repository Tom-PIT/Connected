using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TomPIT.Reflection
{
	internal class ScriptManifestSymbolReferenceComparer : IEqualityComparer<IScriptManifestSymbolReference>
	{
		public bool Equals(IScriptManifestSymbolReference x, IScriptManifestSymbolReference y)
		{
			if (ReferenceEquals(x, y))
				return true;

			if (x is null || y is null)
				return false;

			return x.Location.EndCharacter == y.Location.EndCharacter
				&& x.Location.EndLine == y.Location.EndLine
				&& x.Location.StartCharacter == y.Location.StartCharacter
				&& x.Location.StartLine == y.Location.StartLine
				&& x.Address == y.Address;
		}

		public int GetHashCode([DisallowNull] IScriptManifestSymbolReference obj)
		{
			return GetHashCode();
		}
	}
}