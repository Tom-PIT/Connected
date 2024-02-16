using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;

namespace TomPIT.Compilation
{
	internal class ScriptDescriptor : IScriptDescriptor
	{
		public ScriptRunner<object> Script { get; set; }
		public List<IDiagnostic> Errors { get; set; }
		public Guid MicroService { get; set; }
		public Guid Token { get; set; }
		public string Assembly { get; set; }
		public Guid Component { get; set; }
	}
}
