using Microsoft.CodeAnalysis.Host;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Services
{
	public interface IDefinitionProviderService : IWorkspaceService
	{
		ILocation ProvideDefinition(IPosition position);
	}
}
