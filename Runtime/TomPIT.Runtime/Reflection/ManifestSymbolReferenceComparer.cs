using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TomPIT.Reflection
{
	internal class ManifestSymbolReferenceComparer : IEqualityComparer<IManifestSymbolReference>
	{
		public bool Equals(IManifestSymbolReference x, IManifestSymbolReference y)
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

		public int GetHashCode([DisallowNull] IManifestSymbolReference obj)
		{
			return GetHashCode();
		}
	}
}