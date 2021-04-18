using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TomPIT.Reflection
{
	internal class ScriptManifestPointerComparer : IEqualityComparer<IScriptManifestPointer>
	{
		public bool Equals(IScriptManifestPointer x, IScriptManifestPointer y)
		{
			if (ReferenceEquals(x, y))
				return true;

			if (x is null || y is null)
				return false;

			return x.MicroService == y.MicroService
				&& x.Component == y.Component
				&& x.Element == y.Element;
		}

		public int GetHashCode([DisallowNull] IScriptManifestPointer obj)
		{
			return GetHashCode();
		}
	}
}