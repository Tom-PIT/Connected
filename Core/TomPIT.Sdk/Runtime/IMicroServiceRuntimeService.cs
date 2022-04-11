using Microsoft.AspNetCore.Builder;

using System.Collections.Immutable;

namespace TomPIT.Runtime
{
	public interface IMicroServiceRuntimeService
	{
		void Configure(IApplicationBuilder host);
		ImmutableList<IRuntimeMiddleware> QueryRuntimes();
	}
}
