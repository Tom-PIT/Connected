using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.ActionProviders
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
		protected CSharpEditor Editor => Arguments.Editor as CSharpEditor;
	}
}
