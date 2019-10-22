using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Scripting;

namespace TomPIT.Compilation
{
	internal class ScriptDescriptor : IScriptDescriptor
	{
		public ScriptRunner<object> Script { get; set; }
		public List<IDiagnostic> Errors { get; set; }
		public Guid MicroService { get; set; }
		public Guid Id { get; set; }
		public string Assembly { get; set; }
		public Guid Component { get; set; }
	}
}
