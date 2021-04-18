using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Host;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Services
{
	public interface ICodeLensService : IWorkspaceService
	{
		ImmutableList<ICodeLens> ProvideCodeLens();
	}
}
