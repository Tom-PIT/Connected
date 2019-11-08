using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public interface ICodeActionContext
	{
		List<IMarkerData> Markers { get; }
		string Only { get; }
	}
}
