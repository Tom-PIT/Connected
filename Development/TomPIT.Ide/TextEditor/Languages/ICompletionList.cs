using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public interface ICompletionList
	{
		bool Incomplete { get; }
		List<ICompletionItem> Suggestions { get; }
	}
}
