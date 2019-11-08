using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Razor.Services.ActionProviders
{
	internal abstract class ActionProvider : IActionProvider
	{
		public List<ICodeAction> GetActions(CodeActionProviderArgs e)
		{
			Arguments = e;

			return OnGetActions();
		}

		protected virtual List<ICodeAction> OnGetActions()
		{
			return null;
		}

		protected CodeActionProviderArgs Arguments { get; private set; }
		protected RazorEditor Editor => Arguments.Editor as RazorEditor;
	}
}
