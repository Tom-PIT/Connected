using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Scripting;

namespace TomPIT.Compilation
{
	public interface IScriptDescriptor
	{
		string Assembly { get; }
		ScriptRunner<object> Script { get; }
		List<IDiagnostic> Errors { get; }
		Guid MicroService { get; }
		Guid Id { get; }
		Guid Component { get; }
	}
}
