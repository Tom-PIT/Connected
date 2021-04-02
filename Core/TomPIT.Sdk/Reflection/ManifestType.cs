using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public class ManifestType : ManifestMember, IManifestType
	{
		private List<IManifestMember> _members = null;

		public List<IManifestMember> Members => _members ??= new List<IManifestMember>();
	}
}
