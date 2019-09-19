using System.Collections.Generic;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Ide.TextEditor.CSharp.Services.ActionProviders
{
	internal interface IActionProvider
	{
		List<ICodeAction> GetActions(CodeActionProviderArgs e);
	}
}
