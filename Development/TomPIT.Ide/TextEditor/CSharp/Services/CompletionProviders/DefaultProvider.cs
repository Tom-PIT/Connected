using System.Collections.Generic;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Ide.TextEditor.CSharp.Services.CompletionProviders
{
	internal class DefaultProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var result = new List<ICompletionItem>();
			var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(Editor.Document);
			var completions = service.GetCompletionsAsync(Editor.Document, Editor.Document.GetPosition(Arguments.Position)).Result;

			if (completions == null)
				return result;

			var descriptionCount = 0;

			foreach (var suggestion in completions.Items)
			{
				var returnValue = FromCompletion(service, suggestion, descriptionCount < 25);

				if (returnValue.Item2)
					descriptionCount++;

				result.Add(returnValue.Item1);
			}

			return result;
		}
	}
}
