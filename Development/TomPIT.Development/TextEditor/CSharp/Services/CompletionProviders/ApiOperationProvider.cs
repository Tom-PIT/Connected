using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class ApiOperationProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Api;
		protected override bool IncludeReferences => true;

		protected override void OnProvideItems(List<ICompletionItem> items, IMicroService microService, IComponent component)
		{
			var configuration = Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IApiConfiguration;

			if (configuration == null)
				return;

			var isSelf = Editor.Context.MicroService.Token == microService.Token;

			if (!isSelf && configuration.Scope != ElementScope.Public)
				return;

			foreach (var operation in configuration.Operations)
			{
				if (!isSelf && operation.Scope != ElementScope.Public)
					continue;

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
