using System.Collections.Immutable;

namespace TomPIT.Runtime
{
	public interface IMicroServiceRuntimeService
	{
		ImmutableList<IRuntimeMiddleware> QueryRuntimes();
	}
}
