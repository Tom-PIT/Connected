namespace TomPIT.Middleware.Interop
{
	public interface IMiddlewareProxy
	{
		bool IsDefined(string propertyName);
		bool ContainsValue<T>(string propertyName);
		T GetValue<T>(string propertyName);
	}
}
