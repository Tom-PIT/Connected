using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public interface IWorkspaceEdit
	{
		List<IResourceEdit> Edits { get; }
	}
}
