using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public interface IWorkspaceEdit
	{
		List<IResourceEdit> Edits { get; }
	}
}
