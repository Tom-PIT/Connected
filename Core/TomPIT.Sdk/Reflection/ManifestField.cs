using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ManifestField : ManifestMember, IManifestField
	{
		private List<IManifestAttribute> _attributes = null;
		public bool IsConstant { get; set; }
		public List<IManifestAttribute> Attributes => _attributes ??= new List<IManifestAttribute>();

		public bool IsPublic {get;set;}
	}
}
