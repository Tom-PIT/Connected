using System.Dynamic;

namespace TomPIT.Middleware
{
	public interface IMiddlewareIoC : IMiddlewareComponent, IDynamicMetaObjectProvider
	{
		T GetValue<T>(string propertyName);
		void SetValue<T>(string propertyName, T value);

		void Invoke(string methodName, params object[] args);
		T Invoke<T>(string methodName, params object[] args);
	}
}
