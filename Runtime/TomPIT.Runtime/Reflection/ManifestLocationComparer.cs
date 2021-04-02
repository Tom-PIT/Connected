using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TomPIT.Reflection
{
	internal class ManifestLocationComparer : IEqualityComparer<IManifestSymbolLocation>
	{
		public bool Equals(IManifestSymbolLocation x, IManifestSymbolLocation y)
		{
			if (ReferenceEquals(x, y))
				return true;

			if (x is null || y is null)
				return false;

			return x.EndCharacter == y.EndCharacter
				&& x.EndLine == y.EndLine
				&& x.StartCharacter == y.StartCharacter
				&& x.StartLine == y.StartLine;
		}

		public int GetHashCode([DisallowNull] IManifestSymbolLocation obj)
		{
			return GetHashCode();
		}
	}
}