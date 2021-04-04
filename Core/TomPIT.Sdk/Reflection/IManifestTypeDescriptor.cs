using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public interface IManifestTypeDescriptor
	{
		string Name { get; }
		Dictionary<string, IManifestTypeDescriptor> Members { get; }
		Dictionary<string, IManifestTypeDescriptor> TypeArguments { get; }

		bool IsPrimitive { get; }
		bool IsArray { get; }
	}
}
