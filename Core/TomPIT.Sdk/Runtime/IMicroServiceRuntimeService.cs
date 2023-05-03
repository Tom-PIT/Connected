using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace TomPIT.Runtime
{
	public interface IMicroServiceRuntimeService
	{
		void Configure(IApplicationBuilder host);
		void Configure(IServiceCollection services);
		ImmutableList<IRuntimeMiddleware> QueryRuntimes();
	}
}
