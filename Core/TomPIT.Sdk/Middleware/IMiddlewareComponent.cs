using TomPIT.Data;

namespace TomPIT.Middleware
{
	public interface IMiddlewareComponent : IUniqueValueProvider, IMiddlewareObject
	{
		void Validate();
	}
}
