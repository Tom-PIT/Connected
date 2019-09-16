namespace TomPIT.Middleware.Interop
{
	public interface IOperation<TReturnValue> : IMiddlewareOperation
	{
		TReturnValue Invoke();
		T Invoke<T>();

		string Extender { get; set; }
	}

	public interface IOperation : IMiddlewareOperation
	{
		void Invoke();
	}
}
