using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders
{
	internal interface ICompletionProvider
	{
		List<ICompletionItem> ProvideItems(CompletionProviderArgs e);

		bool WillProvideItems(CompletionProviderArgs e, IReadOnlyCollection<ICompletionItem> existing);
	}
}
