using System.Collections.Generic;

namespace TomPIT.Reflection.Manifests.Entities
{
	public class SettingsManifest : ComponentManifest
	{
		private List<ManifestMember> _types = null;

		public ManifestType Type { get; set; }
		public List<ManifestMember> Types
		{
			get
			{
				if (_types == null)
					_types = new List<ManifestMember>();

				return _types;
			}
		}
	}
}
