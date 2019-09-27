using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Ide.TextEditor.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Ide.TextEditor.Razor.Services.CompletionProviders
{
	internal class DefaultProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var model = Editor.Document.GetSemanticModelAsync().Result;
			var mappedText = SourceText.From(Editor.Text);
			var result = new List<ICompletionItem>();
			var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(Editor.Document);
			var targetLine = TextSpan.FromBounds(0, 0);
			var caret = -1;

			foreach (var token in model.SyntaxTree.GetRoot().DescendantTokens())
			{
				var mappedSpan = token.GetLocation().GetMappedLineSpan();

				if (!mappedSpan.HasMappedPath)
					continue;

				if (mappedSpan.StartLinePosition.Line == Arguments.Position.LineNumber - 1
					&& (mappedSpan.Span.Start.Character <= Arguments.Position.Column - 1 && mappedSpan.Span.End.Character >= Arguments.Position.Column - 1))
				{
					caret = token.Span.End;
					break;
				}
			}

			if (caret == -1)
				return null;

			var completions = service.GetCompletionsAsync(Editor.Document, caret).Result;

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
