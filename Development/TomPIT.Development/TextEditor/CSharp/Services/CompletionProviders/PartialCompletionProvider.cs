using System.Collections.Generic;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class PartialCompletionProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			return base.OnProvideItems();
		}
	}
}
