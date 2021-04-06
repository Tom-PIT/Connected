using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ScriptManifestProperty : ScriptManifestMember, IScriptManifestProperty
	{
		private List<IScriptManifestAttribute> _attributes;
		public bool CanRead {get;set;}

		public bool CanWrite {get;set;}

		public bool IsPublic {get;set;}

		public List<IScriptManifestAttribute> Attributes => _attributes ??= new List<IScriptManifestAttribute>();
	}
}
