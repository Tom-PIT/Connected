using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
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

			ProvideItems(items, Editor.Context.MicroService);

			if (IncludeReferences)
			{
				var references = Editor.Context.Tenant.GetService<IDiscoveryService>().MicroServices.References.Select(Editor.Context.MicroService.Token);

				if (references == null || references.MicroServices.Count == 0)
					return items;

				foreach (var reference in references.MicroServices)
				{
					var microService = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(reference.MicroService);

					if (microService == null)
						continue;

					ProvideItems(items, microService);
				}
			}

			return items;
		}

		private void ProvideItems(List<ICompletionItem> items, IMicroService microService)
		{
			var components = Editor.Context.Tenant.GetService<IComponentService>().QueryComponents(microService.Token, ComponentCategory);

			foreach (var i in components)
				OnProvideItems(items, microService, i);
		}

		protected virtual void OnProvideItems(List<ICompletionItem> items, IMicroService microService, IComponent component)
		{
			AddItem(items, microService, component);
		}

		protected void AddItem(List<ICompletionItem> items, IMicroService microService, IComponent component)
		{
			var text = $"{microService.Name}/{component.Name}";

			items.Add(new CompletionItem
			{
				FilterText = text,
				InsertText = text,
				SortText = text,
				Label = text,
				Kind = CompletionItemKind.Reference
			});
		}
	}
}
