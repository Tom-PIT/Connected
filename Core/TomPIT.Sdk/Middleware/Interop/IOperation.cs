using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Interop
{
	public interface IOperation<TReturnValue> : IMiddlewareOperation
	{
		TReturnValue Invoke();

		T Invoke<T>();

		[CIP(CIP.ExtenderProvider)]
		string Extender { get; set; }
	}

	public interface IOperation : IMiddlewareOperation
	{
		void Invoke();
	}
}
