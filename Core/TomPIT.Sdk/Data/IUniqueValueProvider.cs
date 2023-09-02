using TomPIT.Middleware;

namespace TomPIT.Data
{
	public interface IUniqueValueProvider
	{
		bool IsUnique(IMiddlewareContext context, string propertyName);
	}
}
