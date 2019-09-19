using System.Collections.Generic;
using Microsoft.CodeAnalysis.Host;
using TomPIT.ComponentModel;

namespace TomPIT.Ide.TextEditor.Services
{
	public interface ISyntaxCheckService : IWorkspaceService
	{
		List<IMarkerData> CheckSyntax(ISourceCode sourceCode);
	}
}
