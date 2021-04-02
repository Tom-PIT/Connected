using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ManifestTypeDescriptor : IManifestTypeDescriptor
	{
		private Dictionary<string, IManifestTypeDescriptor> _members;
		private Dictionary<string, IManifestTypeDescriptor> _typeArguments;
		public string Name { get; set; }

		public Dictionary<string, IManifestTypeDescriptor> Members => _members ??= new Dictionary<string, IManifestTypeDescriptor>();

		public Dictionary<string, IManifestTypeDescriptor> TypeArguments => _typeArguments ??= new Dictionary<string, IManifestTypeDescriptor>();

		public bool IsPrimitive { get; set; }

		public bool IsArray { get; set; }

		public bool IsDictionary { get; set; }

		public bool IsTuple { get; set; }

	}
}