using System.Collections.Generic;

namespace TomPIT.Security
{
	public interface IElevationContext
	{
		List<string> Claims { get; }
	}
}
