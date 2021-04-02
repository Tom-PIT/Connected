using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public interface IManifestTypeDescriptor
	{
		public string Name { get; }
		Dictionary<string, IManifestTypeDescriptor> Members { get; }
		Dictionary<string, IManifestTypeDescriptor> TypeArguments { get; }

		bool IsPrimitive { get; }
		bool IsArray { get; }
		bool IsDictionary { get; }
		bool IsTuple { get; }
	}
}
