using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ScriptDocumentation
	{
		private List<ScriptMemberDocumentation> _members;
		public List<ScriptMemberDocumentation> Members => _members ??= new List<ScriptMemberDocumentation>();
	}
}
