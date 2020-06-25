using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Completion;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders
{
	internal class DefaultProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var position = Editor.Document.GetCaret(Arguments.Position);
			var result = new List<ICompletionItem>();
			var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(Editor.Document);
			var completions = service.GetCompletionsAsync(Editor.Document, position, Trigger).Result;

			if (completions == null)
				return result;

			var descriptionCount = 0;

			foreach (var suggestion in completions.Items)
			{
				var returnValue = FromCompletion(service, suggestion, descriptionCount < 25);

				if (result.FirstOrDefault(f => string.Compare(f.Label, returnValue.Item1.Label, false) == 0
					&& f.Kind == returnValue.Item1.Kind
					&& string.Compare(f.InsertText, returnValue.Item1.InsertText, false) == 0) != null)
					continue;

				if (returnValue.Item2)
					descriptionCount++;

				result.Add(returnValue.Item1);
			}

			return result;
		}

		public override bool WillProvideItems(CompletionProviderArgs e, IReadOnlyCollection<ICompletionItem> existing)
		{
			return existing.Count == 0;
		}

		private CompletionTrigger Trigger => Arguments.Context.TriggerCharacter == null ? CompletionTrigger.Invoke : CompletionTrigger.CreateInsertionTrigger(Arguments.Context.TriggerCharacter[0]);
	}
}
