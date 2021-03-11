using System.Collections.Generic;

namespace TomPIT.Reflection.Manifests.Entities
{
	public class ManifestProperty : ManifestTypeDescriptor
	{
		private List<ManifestAttribute> _attributes = null;
		public string Name { get; set; }
		public bool CanRead { get; set; }
		public bool CanWrite { get; set; }
		public string Documentation { get; set; }
		public List<ManifestAttribute> Attributes => _attributes ??= new List<ManifestAttribute>();
	}
}
