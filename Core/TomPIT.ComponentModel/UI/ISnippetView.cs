using TomPIT.Collections;

namespace TomPIT.ComponentModel.UI
{
	public interface ISnippetView
	{
		ListItems<ISnippet> Snippets { get; }
	}
}
