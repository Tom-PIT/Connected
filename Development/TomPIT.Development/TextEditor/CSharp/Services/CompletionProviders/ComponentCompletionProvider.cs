using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextEditor.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextEditor.Languages;
using TomPIT.Reflection;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal abstract class ComponentCompletionProvider : CompletionProvider
	{
		protected abstract string ComponentCategory { get; }
		protected virtual bool IncludeReferences => false;

		protected override List<ICompletionItem> OnProvideItems()
		{
			var items = new List<ICompletionItem>();

			ProvideItems(items, Editor.MicroService.Token);

			if (IncludeReferences)
			{
				var references = Editor.Context.Tenant.GetService<IDiscoveryService>().References(Editor.MicroService.Token);

				if (references == null || references.MicroServices.Count == 0)
					return items;

				foreach (var reference in references.MicroServices)
				{
					var microService = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(reference.MicroService);

					if (microService == null)
						continue;

					ProvideItems(items, microService.Token);
				}
			}

			return items;
		}

		private void ProvideItems(List<ICompletionItem> items, Guid microService)
		{
			var components = Editor.Context.Tenant.GetService<IComponentService>().QueryComponents(microService, ComponentCategory);

			foreach (var i in components)
			{
				var text = $"{Editor.MicroService.Name}/{i.Name}";

				items.Add(new CompletionItem
				{
					FilterText = text,
					InsertText = text,
					SortText = text,
					Label = text,
					Kind = CompletionItemKind.Text
				});
			}
		}
	}
}
