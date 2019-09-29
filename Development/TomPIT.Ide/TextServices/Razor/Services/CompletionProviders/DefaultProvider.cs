using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Razor.Services.CompletionProviders
{
	internal class DefaultProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var model = Editor.Document.GetSemanticModelAsync().Result;
			var mappedText = SourceText.From(Editor.Text);
			var result = new List<ICompletionItem>();
			var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(Editor.Document);
			var caret = -1;

			foreach (var token in model.SyntaxTree.GetRoot().DescendantTokens())
			{
				if (IsInRange(token))
				{
					caret = token.Span.End;

					if (IsInline(token))
						caret--;

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

		private bool IsInline(SyntaxToken token)
		{
			//najprej iščem __o v isti vrstici. če je, potem je inline

			var currentLine = token.GetLocation().GetLineSpan().StartLinePosition;
			var currentToken = token.GetPreviousToken();

			while (true)
			{
				if (currentToken.IsMissing)
					break;

				if (currentToken.GetLocation().GetLineSpan().StartLinePosition.Line != currentLine.Line)
					break;

				if (currentToken.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierToken))
				{
					if (string.Compare(currentToken.ValueText, "__o", false) == 0)
						return true;
				}

				currentToken = currentToken.GetPreviousToken();
			}

			var current = token.Parent;

			while (current != null)
			{
				if (current is BlockSyntax bs)
					return false;

				current = current.Parent;
			}

			return true;
		}

		private bool IsInRange(Microsoft.CodeAnalysis.SyntaxToken token)
		{
			var mappedSpan = token.GetLocation().GetMappedLineSpan();

			if (!mappedSpan.HasMappedPath)
				return false;

			var mappedLine = Arguments.Position.LineNumber - 1;
			var mappedColumn = Arguments.Position.Column - 1;

			return mappedSpan.StartLinePosition.Line == mappedLine && (mappedSpan.Span.Start.Character <= mappedColumn && mappedSpan.Span.End.Character >= mappedColumn);
		}
	}
}
