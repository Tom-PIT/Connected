using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	internal interface IScriptManifestCompiler: ITenantObject
	{
		Guid MicroService { get; }
		Guid Component { get; }
		Guid Element { get; }

		IScriptManifest Manifest { get; }
		SyntaxTree SyntaxTree { get; }
		Microsoft.CodeAnalysis.Compilation Compilation { get; set; }
		SemanticModel Model { get; set; }
		
		IText ResolveScript(string fileName);
		List<IScriptManifestProvider> Providers { get; }
	}
}
