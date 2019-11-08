using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class IoCOperationsProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.IoCContainer;
		protected override bool IncludeReferences => true;

		protected override void OnProvideItems(List<ICompletionItem> items, IMicroService microService, IComponent component)
		{
			var configuration = Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IIoCContainerConfiguration;

			if (configuration == null)
				return;

			foreach (var operation in configuration.Operations)
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
