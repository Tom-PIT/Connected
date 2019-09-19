using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public interface ICommand
	{
		List<string> Arguments { get; }
		string Id { get; }
		string Title { get; }
		string Tooltip { get; }
	}
}
