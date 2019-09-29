using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Razor.Services.ActionProviders
{
	internal interface IActionProvider
	{
		List<ICodeAction> GetActions(CodeActionProviderArgs e);
	}
}
