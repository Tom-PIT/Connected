using System.Collections.Generic;

namespace TomPIT.Reflection.Manifests.Entities
{
	public class ManifestTypeDescriptor
	{
		private List<ManifestTypeDescriptor> _typeArguments = null;

		public string Type { get; set; }
		public bool IsArray { get; set; }
		public bool IsScriptClass { get; set; }
		public List<ManifestTypeDescriptor> TypeArguments => _typeArguments ??= new List<ManifestTypeDescriptor>();

	}
}
