using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public interface ICompletionList
	{
		bool Incomplete { get; }
		List<ICompletionItem> Suggestions { get; }
	}
}
