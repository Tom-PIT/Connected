using System.Collections.Generic;

namespace TomPIT.Ide.Analysis.Hovering
{
	public interface IHoverInfo
	{
		IRange Range { get; }
		List<IHoverLine> Content { get; }
	}
}
