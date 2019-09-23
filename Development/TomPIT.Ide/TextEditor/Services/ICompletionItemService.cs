using Microsoft.CodeAnalysis.Host;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Ide.TextEditor.Services
{
	public interface ICompletionItemService : IWorkspaceService
	{
		ICompletionList ProvideCompletionItems(IPosition position, ICompletionContext context);
	}
}
