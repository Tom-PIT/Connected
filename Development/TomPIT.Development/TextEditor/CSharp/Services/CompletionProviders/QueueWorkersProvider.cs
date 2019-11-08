using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class QueueWorkersProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Queue;

		protected override void OnProvideItems(List<ICompletionItem> items, IMicroService microService, IComponent component)
		{
			if (!(Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IQueueConfiguration configuration))
				return;

			foreach (var operation in configuration.Workers)
			{
				var text = $"{microService.Name}/{component.Name}/{operation.Name}";

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
