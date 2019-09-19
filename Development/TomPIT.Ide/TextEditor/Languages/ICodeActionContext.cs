using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public interface ICodeActionContext
	{
		List<IMarkerData> Markers { get; }
		string Only { get; }
	}
}
