using System.Collections.Generic;

namespace TomPIT.Reflection.Manifests.Entities
{
	public class IoCContainerManifest : ComponentManifest
	{
		private List<IoCOperationManifest> _operations = null;
		private List<ManifestMember> _types = null;

		public List<IoCOperationManifest> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new List<IoCOperationManifest>();

				return _operations;
			}
		}

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
