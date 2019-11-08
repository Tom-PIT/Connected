using System.Collections.Generic;
using TomPIT.Development.Navigation;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class RouteKeyProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var keys = Editor.Context.Tenant.GetService<INavigationDesignService>().QueryRouteKeys(Editor.Context.MicroService.Token);
			var r = new List<ICompletionItem>();

			foreach (var key in keys)
			{
				r.Add(new CompletionItem
				{
					Label = key.RouteKey,
					InsertText = key.RouteKey,
					Detail = key.Template,
					FilterText = key.RouteKey,
					SortText = key.RouteKey,
					Kind = CompletionItemKind.Reference
				});
			}

			return r;
		}
	}
}
