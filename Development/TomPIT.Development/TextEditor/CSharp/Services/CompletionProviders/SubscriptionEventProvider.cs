using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class SubscriptionEventProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Subscription;
		protected override void OnProvideItems(List<ICompletionItem> items, IMicroService microService, IComponent component)
		{
			if (!(Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is ISubscriptionConfiguration config))
				return;

			foreach (var ev in config.Events)
			{
				if (string.IsNullOrWhiteSpace(ev.Name))
					continue;

				var ms = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(ev.Configuration().MicroService());
				var text = $"{ms.Name}/{component.Name}/{ev.Name}";

				items.Add(new CompletionItem
				{
					Kind = CompletionItemKind.Reference,
					Label = text,
					FilterText = text,
					InsertText = text,
					SortText = text
				});
			}
		}
	}
}
