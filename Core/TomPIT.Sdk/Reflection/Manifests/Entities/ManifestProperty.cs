using System.Collections.Generic;

namespace TomPIT.Reflection.Manifests.Entities
{
	public class ManifestProperty
	{
		private List<ManifestAttribute> _attributes = null;
		public string Name { get; set; }
		public string Type { get; set; }
		public string Documentation { get; set; }

		public List<ManifestAttribute> Validation
		{
			get
			{
				if (_attributes == null)
					_attributes = new List<ManifestAttribute>();

				return _attributes;
			}
		}

	}
}
