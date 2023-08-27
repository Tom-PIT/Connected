using System;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
    public interface IApiDependencyInjectionObject : IMiddlewareObject
    {
        IMiddlewareOperation Operation { get; }
        [Obsolete("Please use async method")]
        void Validate();
        [Obsolete("Please use async method")]
        void Authorize();
        [Obsolete("Please use async method")]
        void Commit();
        [Obsolete("Please use async method")]
        void Rollback();

        Task ValidateAsync();
        Task AuthorizeAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
    public interface IApiDependencyInjectionMiddleware : IApiDependencyInjectionObject
    {
        [Obsolete("Please use async method")]
        void Invoke(object e);

        Task InvokeAsync(object e);
    }

    public interface IApiDependencyInjectionMiddleware<T> : IApiDependencyInjectionObject
    {
        [Obsolete("Please use async method")]
        T Invoke(T e);
        [Obsolete("Please use async method")]
        T Authorize(T e);

        Task<T> InvokeAsync(T e);
        Task<T> AuthorizeAsync(T e);
    }
}
