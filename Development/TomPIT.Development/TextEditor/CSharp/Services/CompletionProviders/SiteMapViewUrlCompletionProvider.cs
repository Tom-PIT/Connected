using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class SiteMapViewUrlCompletionProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var result = new List<ICompletionItem>();
			var configs = Editor.Context.Tenant.GetService<IComponentService>().QueryConfigurations(Editor.Context.MicroService.Token, ComponentCategories.View);

			foreach (var config in configs)
			{
				if (!(config is IViewConfiguration view))
					continue;

				var viewName = $"{Editor.Context.MicroService.Name}/{view.ComponentName()}";

				result.Add(new CompletionItem
				{
					FilterText = viewName,
					InsertText = viewName,
					Label = viewName,
					SortText = viewName,
					Kind = CompletionItemKind.Text
				});
			}

			return result;
		}
	}
}
