using Microsoft.Extensions.DependencyInjection;
using System;
using TomPIT.Middleware;

namespace TomPIT.Runtime
{
	public interface IRuntimeMiddleware : IMiddlewareObject
	{
		void Initialize(RuntimeInitializeArgs e);
		void Configure(IServiceCollection services);
		[Obsolete("Please use Resolver.ResolveUrl instead.")]
		IRuntimeUrl ResolveUrl(RuntimeUrlKind kind);

		IRuntimeResolver Resolver { get; }

		IRuntimeViewModifier ViewModifier { get; }
	}
}
