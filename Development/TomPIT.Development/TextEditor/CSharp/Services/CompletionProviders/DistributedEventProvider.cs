using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class DistributedEventProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.DistributedEvent;

		protected override void OnProvideItems(List<ICompletionItem> items, IMicroService microService, IComponent component)
		{
			if (!(Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IDistributedEventsConfiguration configuration))
				return;

			foreach (var ev in configuration.Events)
			{
				var text = $"{microService.Name}/{component.Name}/{ev.Name}";

				items.Add(new CompletionItem
				{
					FilterText = text,
					InsertText = text,
					Kind = CompletionItemKind.Reference,
					Label = text,
					SortText = text
				});
			}
		}
	}
}
