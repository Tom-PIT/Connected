using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class MediaProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Media;
		protected override bool IncludeReferences => true;
		protected override void OnProvideItems(List<ICompletionItem> items, IMicroService microService, IComponent component)
		{
			if (!(Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IMediaResourcesConfiguration configuration))
				return;

			foreach (var i in configuration.Files)
				ProvideItems(microService, component.Name, i, items);

			foreach (var i in configuration.Folders)
				ProvideItems(microService, component.Name, i, items);
		}

		private void ProvideItems(IMicroService microService, string path, IMediaResourceFile file, List<ICompletionItem> items)
		{
			if (string.IsNullOrWhiteSpace(file.FileName))
				return;

			var text = $"{microService.Name}/{path}/{file.FileName}";

			items.Add(new CompletionItem
			{
				FilterText = text,
				InsertText = text,
				Kind = CompletionItemKind.Reference,
				Label = text,
				SortText = text
			});
		}

		private void ProvideItems(IMicroService microService, string path, IMediaResourceFolder folder, List<ICompletionItem> items)
		{
			if (string.IsNullOrWhiteSpace(folder.Name))
				return;

			path = $"{path}/{folder.Name}";

			foreach (var i in folder.Files)
				ProvideItems(microService, path, i, items);

			foreach (var i in folder.Folders)
				ProvideItems(microService, path, i, items);
		}
	}
}
