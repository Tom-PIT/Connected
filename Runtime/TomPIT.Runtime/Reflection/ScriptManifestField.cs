using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ScriptManifestField : ScriptManifestMember, IScriptManifestField
	{
		private List<IScriptManifestAttribute> _attributes;
		public bool IsConstant {get;set;}

		public bool IsPublic {get;set;}

		public List<IScriptManifestAttribute> Attributes => _attributes ??= new List<IScriptManifestAttribute>();
	}
}
