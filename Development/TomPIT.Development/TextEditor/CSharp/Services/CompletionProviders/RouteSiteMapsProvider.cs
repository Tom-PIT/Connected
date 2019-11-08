using System.Collections.Generic;
using TomPIT.Development.Navigation;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class RouteSiteMapsProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var keys = Editor.Context.Tenant.GetService<INavigationDesignService>().QuerySiteMapKeys(Editor.Context.MicroService.Token);
			var r = new List<ICompletionItem>();

			foreach (var key in keys)
			{
				r.Add(new CompletionItem
				{
					Label = key,
					InsertText = key,
					FilterText = key,
					Kind = CompletionItemKind.Reference,
					SortText = key
				});
			}
			return r;
		}
	}
}
