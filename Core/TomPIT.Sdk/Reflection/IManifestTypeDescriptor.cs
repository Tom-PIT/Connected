using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public interface IManifestTypeDescriptor
	{
		string Documentation { get; }
		string Name { get; }
		Dictionary<string, IManifestTypeDescriptor> Members { get; }
		List<IManifestTypeDescriptor> TypeArguments { get; }

		bool IsPrimitive { get; }
		bool IsArray { get; }
	}
}
