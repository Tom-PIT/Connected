using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection.Manifests.Entities
{
	public class ApiManifest : ComponentManifest
	{
		private List<ApiOperationManifest> _operations = null;
		private List<ManifestMember> _types = null;

		public ElementScope Scope { get; set; }

		public List<ApiOperationManifest> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new List<ApiOperationManifest>();

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
