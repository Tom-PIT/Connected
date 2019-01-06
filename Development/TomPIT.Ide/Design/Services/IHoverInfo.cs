using System.Collections.Generic;

namespace TomPIT.Design.Services
{
	public interface IHoverInfo
	{
		IRange Range { get; }
		List<IHoverLine> Content { get; }
	}
}
