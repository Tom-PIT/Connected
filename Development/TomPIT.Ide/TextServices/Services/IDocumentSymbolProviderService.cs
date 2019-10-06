using System.Collections.Generic;
using Microsoft.CodeAnalysis.Host;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Services
{
	public interface IDocumentSymbolProviderService : IWorkspaceService
	{
		List<IDocumentSymbol> ProvideDocumentSymbols();
	}
}
