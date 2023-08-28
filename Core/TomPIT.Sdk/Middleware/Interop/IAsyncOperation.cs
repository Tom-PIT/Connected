using System.Threading.Tasks;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Interop;

public interface IAsyncOperation<TReturnValue> : IMiddlewareOperation
{
    Task<TReturnValue> Invoke(IMiddlewareContext context);

    Task<T> Invoke<T>(IMiddlewareContext context);

    Task<TReturnValue> Invoke();

    Task<T> Invoke<T>();

    [CIP(CIP.ExtenderProvider)]
    string Extender { get; set; }
}

public interface IAsyncOperation : IMiddlewareOperation
{
    Task Invoke();
    Task Invoke(IMiddlewareContext context);
}
