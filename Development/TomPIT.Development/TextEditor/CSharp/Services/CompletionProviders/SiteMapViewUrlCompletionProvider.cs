using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Ide.TextEditor.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class SiteMapViewUrlCompletionProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var result = new List<ICompletionItem>();
			var configs = Editor.Context.Tenant.GetService<IComponentService>().QueryConfigurations(Editor.MicroService.Token, ComponentCategories.View);

			foreach (var config in configs)
			{
				if (!(config is IViewConfiguration view) || string.IsNullOrWhiteSpace(view.Url))
					continue;

				var viewName = $"{Editor.MicroService.Name}/{view.ComponentName()}";

				result.Add(new CompletionItem
				{
					FilterText = viewName,
					InsertText = view.Url,
					Label = viewName,
					SortText = viewName,
					Kind = CompletionItemKind.Text
				});
			}

			return result;
		}
	}
}
