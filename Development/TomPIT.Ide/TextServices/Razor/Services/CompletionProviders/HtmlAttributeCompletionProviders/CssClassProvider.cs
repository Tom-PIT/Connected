using System.Collections.Generic;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.UI.Theming;

namespace TomPIT.Ide.TextServices.Razor.Services.CompletionProviders.HtmlAttributeCompletionProviders
{
	internal class CssClassProvider : HtmlAttributeProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var classes = Editor.Context.Tenant.GetService<IStylesheetService>().QueryClasses(Editor.Context.MicroService.Token, true);
			var result = new List<ICompletionItem>();

			foreach (var @class in classes)
			{
				var label = $"{@class.MicroService}/{@class.Theme}/{@class.Name}";

				result.Add(new CompletionItem
				{
					FilterText = label,
					InsertText = @class.Name,
					Kind = CompletionItemKind.Value,
					Label = label,
					SortText = label
				});
			}

			return result;
		}

		public override bool WillProvideItems(CompletionProviderArgs e, IReadOnlyCollection<ICompletionItem> existing)
		{
			return string.Compare(AttributeName, "class", true) == 0;
		}
	}
}
