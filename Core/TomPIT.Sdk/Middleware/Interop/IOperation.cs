using System.Threading.Tasks;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Interop
{
    public interface IOperation<TReturnValue> : IMiddlewareOperation
    {
        TReturnValue? Invoke(IMiddlewareContext? context);

        T? Invoke<T>(IMiddlewareContext? context);

        TReturnValue? Invoke();

        T? Invoke<T>();

        Task<TReturnValue?> InvokeAsync(IMiddlewareContext? context);

        Task<T?> InvokeAsync<T>(IMiddlewareContext? context);

        Task<TReturnValue?> InvokeAsync();

        Task<T?> InvokeAsync<T>();

        [CIP(CIP.ExtenderProvider)]
        string? Extender { get; set; }
    }

    public interface IOperation : IMiddlewareOperation
    {
        void Invoke();
        void Invoke(IMiddlewareContext? context);

        Task InvokeAsync();
        Task InvokeAsync(IMiddlewareContext? context);
    }
}
