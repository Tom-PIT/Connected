using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public interface ICodeAction
	{
		ICommand Command { get; }
		List<IMarkerData> Diagnostics { get; }
		IWorkspaceEdit Edit { get; }
		bool IsPreferred { get; }
		string Kind { get; }
		string Title { get; }
	}
}
