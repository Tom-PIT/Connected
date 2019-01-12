using Microsoft.AspNetCore.Builder;

namespace TomPIT.Rest
{
	public static class MiddlewareExtensions
	{
		public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<ApiHandler>();
		}
	}
}
