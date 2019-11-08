using System.Collections.Generic;

namespace TomPIT.Reflection.Manifests.Entities
{
	public class ManifestMember
	{
		private List<ManifestAttribute> _attributes = null;
		private List<ManifestProperty> _properties = null;
		public string Type { get; set; }
		public string Documentation { get; set; }
		public List<ManifestAttribute> Attributes
		{
			get
			{
				if (_attributes == null)
					_attributes = new List<ManifestAttribute>();

				return _attributes;
			}
		}

		public List<ManifestProperty> Properties
		{
			get
			{
				if (_properties == null)
					_properties = new List<ManifestProperty>();

				return _properties;
			}
		}
	}
}
