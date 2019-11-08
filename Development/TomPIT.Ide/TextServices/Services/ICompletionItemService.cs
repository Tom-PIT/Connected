using Microsoft.CodeAnalysis.Host;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Services
{
	public interface ICompletionItemService : IWorkspaceService
	{
		ICompletionList ProvideCompletionItems(IPosition position, ICompletionContext context);
	}
}
