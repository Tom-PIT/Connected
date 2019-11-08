using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class DataHubEndpointProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.DataHub;

		protected override void OnProvideItems(List<ICompletionItem> items, IMicroService microService, IComponent component)
		{
			if (!(Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IDataHubConfiguration configuration))
				return;

			foreach (var endpoint in configuration.Endpoints)
			{
				var text = $"{microService.Name}/{component.Name}/{endpoint.Name}";

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
