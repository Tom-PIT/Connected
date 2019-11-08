using Microsoft.CodeAnalysis.Host;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Services
{
	public interface IDeclarationProviderService : IWorkspaceService
	{
		ILocation ProvideDeclaration(IPosition position);
	}
}
