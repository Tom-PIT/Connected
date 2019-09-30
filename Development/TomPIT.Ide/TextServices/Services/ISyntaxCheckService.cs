using System.Collections.Generic;
using Microsoft.CodeAnalysis.Host;
using TomPIT.ComponentModel;

namespace TomPIT.Ide.TextServices.Services
{
	public interface ISyntaxCheckService : IWorkspaceService
	{
		List<IMarkerData> CheckSyntax(IText sourceCode);
	}
}
