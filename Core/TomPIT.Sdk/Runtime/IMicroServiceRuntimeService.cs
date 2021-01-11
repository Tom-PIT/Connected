using System.Collections.Generic;

namespace TomPIT.Runtime
{
	public interface IMicroServiceRuntimeService
	{
		List<IRuntimeMiddleware> QueryRuntimes();
	}
}
