using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;

namespace TomPIT.Compilation
{
	public interface IScriptDescriptor
	{
		string Assembly { get; }
		ScriptRunner<object> Script { get; }
		List<IDiagnostic> Errors { get; }
		Guid MicroService { get; }
		Guid Token { get; }
		Guid Component { get; }
	}
}
