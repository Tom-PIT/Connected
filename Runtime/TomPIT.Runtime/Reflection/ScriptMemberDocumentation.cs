using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ScriptMemberDocumentation
	{
		private List<ScriptMemberDocumentation> _members;

		public string Name { get; set; }

		public string Documentation { get; set; }

		public List<ScriptMemberDocumentation> Members => _members ??= new List<ScriptMemberDocumentation>();
	}
}