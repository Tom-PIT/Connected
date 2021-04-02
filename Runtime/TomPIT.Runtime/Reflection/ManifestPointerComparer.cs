using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TomPIT.Reflection
{
	internal class ManifestPointerComparer : IEqualityComparer<IManifestPointer>
	{
		public bool Equals(IManifestPointer x, IManifestPointer y)
		{
			if (ReferenceEquals(x, y))
				return true;

			if (x is null || y is null)
				return false;

			return x.MicroService == y.MicroService
				&& x.Component == y.Component
				&& x.Element == y.Element;
		}

		public int GetHashCode([DisallowNull] IManifestPointer obj)
		{
			return GetHashCode();
		}
	}
}