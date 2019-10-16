using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders
{
	internal class InteropCompletionProvider : CompletionProvider
	{
		public override bool WillProvideItems(CompletionProviderArgs e, IReadOnlyCollection<ICompletionItem> existing)
		{
			return existing.Count == 0;
		}

		protected override List<ICompletionItem> OnProvideItems()
		{
			var span = Editor.Document.GetSpan(Arguments.Position);
			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(span);

			return base.OnProvideItems();
		}
	}
}
