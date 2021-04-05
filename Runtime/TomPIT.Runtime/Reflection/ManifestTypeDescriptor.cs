using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ManifestTypeDescriptor : IManifestTypeDescriptor
	{
		private Dictionary<string, IManifestTypeDescriptor> _members;
		private List<IManifestTypeDescriptor> _typeArguments;
		public string Name { get; set; }

		public Dictionary<string, IManifestTypeDescriptor> Members => _members ??= new Dictionary<string, IManifestTypeDescriptor>();

		public List<IManifestTypeDescriptor> TypeArguments => _typeArguments ??= new List<IManifestTypeDescriptor>();

		public bool IsPrimitive { get; set; }

		public bool IsArray { get; set; }

	}
}