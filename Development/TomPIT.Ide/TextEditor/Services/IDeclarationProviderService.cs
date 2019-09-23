using Microsoft.CodeAnalysis.Host;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Ide.TextEditor.Services
{
	public interface IDeclarationProviderService : IWorkspaceService
	{
		ILocation ProvideDeclaration(IPosition position);
	}
}
