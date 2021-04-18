namespace TomPIT.Middleware.Interop
{
	public interface IMiddlewareProxy: IMiddlewareObject
	{
		bool IsDefined(string propertyName);
		bool ContainsValue<T>(string propertyName);
		T GetValue<T>(string propertyName);

		object Proxy { get; set; }
	}
}
