using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection.Manifests.Entities
{
	public class ApiOperationManifest : ManifestType
	{
		private List<ManifestType> _extenders = null;
		public ElementScope Scope { get; set; }
		public ManifestType ReturnType { get; set; }

		public bool Async { get; set; }
		public bool SupportsTransaction { get; set; }
		public List<ManifestType> Extenders
		{
			get
			{
				if (_extenders == null)
					_extenders = new List<ManifestType>();

				return _extenders;
			}
		}
	}
}
