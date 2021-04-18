using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ScriptManifestType : ScriptManifestMember, IScriptManifestType
	{
		private List<IScriptManifestMember> _members;

		public List<IScriptManifestMember> Members => _members ??= new List<IScriptManifestMember>();
	}
}
