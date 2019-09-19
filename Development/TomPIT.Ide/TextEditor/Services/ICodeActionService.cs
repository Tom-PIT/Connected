using System.Collections.Generic;
using Microsoft.CodeAnalysis.Host;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Ide.TextEditor.Services
{
	public interface ICodeActionService : IWorkspaceService
	{
		List<ICodeAction> ProvideCodeActions(IRange range, ICodeActionContext context);
	}
}
